#
# Copyright (c) 2022-present, Trail of Bits, Inc.
# All rights reserved.
#
# This source code is licensed in accordance with the terms specified in
# the LICENSE file found in the root directory of this source tree.
#

#
# Source:  https://huggingface.co/databricks/dolly-v1-6b
#
import time
from argparse import ArgumentParser, BooleanOptionalAction
from generator_config import dolly_config
from transformers import AutoModelForCausalLM, AutoTokenizer, pipeline
import torch

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
    config.pipeline = pipeline(
        task="text-generation",
        model=model,
        tokenizer=tokenizer,
        max_new_tokens=config.max_new_tokens)

def generate(config, prompt):
    print(f'generate: Generating from prompt len={len(prompt)}')
    if len(prompt) > MODEL_MAX_PROMPT_LEN:
        raise Exception(f'Prompt is too long, max={MODEL_MAX_PROMPT_LEN}')
    with torch.no_grad():
        return config.pipeline(prompt)[0]['generated_text']

if __name__ == "__main__":
    parser = ArgumentParser()
    parser.add_argument("--model-location", type=str, required=True)
    parser.add_argument("--prompt-location", type=str, required=True)
    parser.add_argument("--max-new-tokens", type=int, default=DEFAULT_NEW_TOKENS, required=False)
    parser.add_argument("--temperature", type=float, default=0.0, required=False)
    parser.add_argument("--force-cpu", action=BooleanOptionalAction, type=bool, default=False, required=False)
    args = parser.parse_args()
    config = dolly_config(
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