#
# Copyright (c) 2022-present, Trail of Bits, Inc.
# All rights reserved.
#
# This source code is licensed in accordance with the terms specified in
# the LICENSE file found in the root directory of this source tree.
#

class generator_base_config:
    def __init__(self, model_path, max_new_tokens, temperature, force_cpu, top_p=0.0, presence_penalty=0.0, frequency_penalty=0.0):
        self.model_path = model_path
        self.max_new_tokens = max_new_tokens
        self.temperature = temperature
        self.top_p = top_p
        self.presence_penalty = presence_penalty
        self.frequency_penalty = frequency_penalty
        self.force_cpu = force_cpu
        self.model = None
        self.tokenizer = None
        self.pipeline = None

class dolly_config(generator_base_config):
    def __init__(self, *args):
        super().__init__(*args)

class stable_lm_config(generator_base_config):
    def __init__(self, *args):
        super().__init__(*args)
