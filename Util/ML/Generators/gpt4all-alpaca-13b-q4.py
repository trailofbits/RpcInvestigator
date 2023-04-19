#
# Copyright (c) 2022-present, Trail of Bits, Inc.
# All rights reserved.
#
# This source code is licensed in accordance with the terms specified in
# the LICENSE file found in the root directory of this source tree.
#

# 
# Status 4.17.2023 : gpt4all python bindings lag significantly behind
# the c++ ones, and their gpt-j based model is in a state of flux.
# The output from the model below is junk.
#

#
# Source:
#   https://huggingface.co/eachadea/ggml-gpt4-x-alpaca-13b-native-4bit
#   https://github.com/nomic-ai/pyllamacpp
#
import time
from argparse import ArgumentParser
from pyllamacpp.model import Model
from generator_config import generator_base_config
import torch

MODEL_MAX_PROMPT_LEN = 508 # as indicated by llamacpp
DEFAULT_NEW_TOKENS = 100
device = 'cpu'
has_cuda = torch.cuda.is_available()
if has_cuda:
    device = 'cuda:0'
    torch.cuda.empty_cache()
    print(f'Using GPU device {torch.zeros(1).cuda().device}')
else:
    print('Using CPU')

def init_model(generator_base_config):
    print(f'init_model: Initializing model {generator_base_config.model_path}')
    #
    # For parameters to Model(), see:
    #   https://nomic-ai.github.io/pyllamacpp/#pyllamacpp.constants.LLAMA_CONTEXT_PARAMS_SCHEMA
    #
    generator_base_config.model = Model(
        generator_base_config.model_path,
        n_ctx=generator_base_config.max_new_tokens)

def generate(generator_base_config, prompt):
    print(f'generate: Generating from prompt len={len(prompt)}')
    if len(prompt) > MODEL_MAX_PROMPT_LEN:
        raise Exception(f'Prompt is too long, max={MODEL_MAX_PROMPT_LEN}')
    #
    # For parameters to generate(), see:
    #   https://nomic-ai.github.io/pyllamacpp/#pyllamacpp.constants.GPT_PARAMS_SCHEMA
    #
    output=''
    def new_text_callback(text):
        nonlocal output
        output += text

    with torch.no_grad():
        _ = generator_base_config.model.generate(
            prompt,
            n_predict=generator_base_config.max_new_tokens,
            repeat_penalty=generator_base_config.frequency_penalty,
            top_p=generator_base_config.top_p,
            temp=generator_base_config.temperature,
            new_text_callback=new_text_callback,
            n_threads=8)
        return output

if __name__ == "__main__":
    parser = ArgumentParser()
    parser.add_argument("--model-location", type=str, required=True)
    parser.add_argument("--prompt-location", type=str, required=True)
    parser.add_argument("--max-new-tokens", type=int, default=DEFAULT_NEW_TOKENS, required=False)
    parser.add_argument("--temperature", type=float, default=0.0, required=False)
    parser.add_argument("--top-p", type=float, default=0.0, required=False)
    parser.add_argument("--presence-penalty", type=float, default=0.0, required=False)
    parser.add_argument("--frequency-penalty", type=float, default=0.0, required=False)

    args = parser.parse_args()
    config = generator_base_config(
        args.model_location,
        args.max_new_tokens,
        args.temperature,
        args.top_p,
        args.presence_penalty,
        args.frequency_penalty,
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