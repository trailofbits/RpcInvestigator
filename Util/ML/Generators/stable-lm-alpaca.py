#
# Copyright (c) 2022-present, Trail of Bits, Inc.
# All rights reserved.
#
# This source code is licensed in accordance with the terms specified in
# the LICENSE file found in the root directory of this source tree.
#

#
# Source:  https://huggingface.co/stabilityai/stablelm-tuned-alpha-3b
#
import time
from argparse import ArgumentParser, BooleanOptionalAction
from generator_config import stable_lm_config
from transformers import AutoModelForCausalLM, AutoTokenizer, StoppingCriteria, StoppingCriteriaList
import torch

class StopOnTokens(StoppingCriteria):
    def __call__(self, input_ids: torch.LongTensor, scores: torch.FloatTensor, **kwargs) -> bool:
        stop_ids = [50278, 50279, 50277, 1, 0]
        for stop_id in stop_ids:
            if input_ids[0][-1] == stop_id:
                return True
        return False

system_prompt = """<|SYSTEM|># StableLM Tuned (Alpha version)"""
MODEL_MAX_PROMPT_LEN = 512 # this is a guess
DEFAULT_NEW_TOKENS = 100

def get_device(config):
    has_cuda = torch.cuda.is_available()
    if config.force_cpu:
        print('Forcing use of CPU for inference.')
        return 'cpu'
    elif has_cuda:
        torch.cuda.empty_cache()
        print(f'Using GPU device {torch.zeros(1).cuda().device}')
        return 'cuda'
    print('No GPU detected, using CPU.')
    return 'cpu'

def init_model(config):
    device = get_device(config)
    if device == 'cpu':
        model = AutoModelForCausalLM.from_pretrained(config.model_path).to('cpu')
        tokenizer = AutoTokenizer.from_pretrained(config.model_path)
    else:
        model = AutoModelForCausalLM.from_pretrained(config.model_path, device_map="auto")
        tokenizer = AutoTokenizer.from_pretrained(config.model_path, device_map="auto")
    print(f"Mem needed: {model.get_memory_footprint() / 1024 / 1024 / 1024:.2f} GB")
    config.model = model
    config.tokenizer = tokenizer

def generate(config, prompt):
    device = get_device(config)
    print(f'generate: Generating from prompt len={len(prompt)}')
    if len(prompt) > MODEL_MAX_PROMPT_LEN:
        raise Exception(f'Prompt is too long, max={MODEL_MAX_PROMPT_LEN}')
    with torch.no_grad():
        prompt = f"{system_prompt}<|USER|>{prompt}<|ASSISTANT|>"
        inputs = config.tokenizer(prompt, return_tensors="pt").to(device)
        tokens = config.model.generate(
        **inputs,
        max_new_tokens=config.max_new_tokens,
        temperature=config.temperature,
        do_sample=True,
        stopping_criteria=StoppingCriteriaList([StopOnTokens()])
        )
        # Extract out only the completion tokens
        completion_tokens = tokens[0][inputs['input_ids'].size(1):]
        return config.tokenizer.decode(completion_tokens, skip_special_tokens=True)

if __name__ == "__main__":
    parser = ArgumentParser()
    parser.add_argument("--model-location", type=str, required=True)
    parser.add_argument("--prompt-location", type=str, required=True)
    parser.add_argument("--max-new-tokens", type=int, default=DEFAULT_NEW_TOKENS, required=False)
    parser.add_argument("--temperature", type=float, default=0.1, required=False)
    parser.add_argument("--force-cpu", action=BooleanOptionalAction, type=bool, default=False, required=False)
    args = parser.parse_args()
    config = stable_lm_config(
        args.model_location,
        args.max_new_tokens,
        args.temperature,
        args.force_cpu
    )
    
    from pathlib import Path
    print(f'Loading prompt from file {args.prompt_location}')
    prompt = Path(args.prompt_location).read_text()
    print(f'Initializing model {args.model_location}')
    model = init_model(config)
    print('Generating...')
    start = time.time()
    generation = generate(config, prompt)
    print(f"Done in {time.time() - start:.2f}s")
    print(generation)