# Copyright 2015 gRPC authors.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
"""The Python implementation of the GRPC helloworld.Greeter server."""

from concurrent import futures
import logging

import math
import grpc
import prime_pb2
import prime_pb2_grpc

_SERVER_PORT = '50051'
_LOGGER = logging.getLogger(__name__)


def is_prime(n):
    for i in range(2, int(math.ceil(math.sqrt(n)))):
        if n % i == 0:
            return False
    else:
        return True


class PrimeChecker(prime_pb2_grpc.PrimeCheckerServicer):

    def check(self, request, context):
        _LOGGER.info('Determining primality of %s', request.candidate)
        return prime_pb2.Primality(isPrime=is_prime(request.candidate))


def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    prime_pb2_grpc.add_PrimeCheckerServicer_to_server(PrimeChecker(), server)
    server.add_insecure_port(f'[::]:{_SERVER_PORT}')
    server.start()
    server.wait_for_termination()


if __name__ == '__main__':
    logging.basicConfig()
    serve()
