from __future__ import print_function

import logging

import grpc
import prime_pb2
import prime_pb2_grpc


def run():
    # NOTE(gRPC Python Team): .close() is possible on a channel and should be
    # used in circumstances in which the with statement does not fit the needs
    # of the code.
    with grpc.insecure_channel('0.0.0.0:50053') as channel:
        stub = prime_pb2_grpc.PrimeCheckerStub(channel)
        response = stub.check(prime_pb2.PrimeCandidate(candidate=2))
    print("Greeter client received: ", response)


if __name__ == '__main__':
    logging.basicConfig()
    run()
