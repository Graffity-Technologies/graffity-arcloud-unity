# Generated by the gRPC Python protocol compiler plugin. DO NOT EDIT!
"""Client and server classes corresponding to protobuf-defined services."""
import grpc

import protos.image.image_pb2 as image__pb2


class ImageStub(object):
    """Missing associated documentation comment in .proto file."""

    def __init__(self, channel):
        """Constructor.

        Args:
            channel: A grpc.Channel.
        """
        self.SendImage = channel.unary_unary(
            '/image.Image/SendImage',
            request_serializer=image__pb2.ImageRequest.SerializeToString,
            response_deserializer=image__pb2.ImageResponse.FromString,
        )


class ImageServicer(object):
    """Missing associated documentation comment in .proto file."""

    def SendImage(self, request, context):
        """Missing associated documentation comment in .proto file."""
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')


def add_ImageServicer_to_server(servicer, server):
    rpc_method_handlers = {
        'SendImage': grpc.unary_unary_rpc_method_handler(
            servicer.SendImage,
            request_deserializer=image__pb2.ImageRequest.FromString,
            response_serializer=image__pb2.ImageResponse.SerializeToString,
        ),
    }
    generic_handler = grpc.method_handlers_generic_handler(
        'image.Image', rpc_method_handlers)
    server.add_generic_rpc_handlers((generic_handler,))

 # This class is part of an EXPERIMENTAL API.


class Image(object):
    """Missing associated documentation comment in .proto file."""

    @staticmethod
    def SendImage(request,
                  target,
                  options=(),
                  channel_credentials=None,
                  call_credentials=None,
                  insecure=False,
                  compression=None,
                  wait_for_ready=None,
                  timeout=None,
                  metadata=None):
        return grpc.experimental.unary_unary(request, target, '/image.Image/SendImage',
                                             image__pb2.ImageRequest.SerializeToString,
                                             image__pb2.ImageResponse.FromString,
                                             options, channel_credentials,
                                             insecure, call_credentials, compression, wait_for_ready, timeout, metadata)


class ImageStreamStub(object):
    """Missing associated documentation comment in .proto file."""

    def __init__(self, channel):
        """Constructor.

        Args:
            channel: A grpc.Channel.
        """
        self.SendImage = channel.stream_stream(
            '/image.ImageStream/SendImage',
            request_serializer=image__pb2.ImageRequest.SerializeToString,
            response_deserializer=image__pb2.ImageResponse.FromString,
        )


class ImageStreamServicer(object):
    """Missing associated documentation comment in .proto file."""

    def SendImage(self, request_iterator, context):
        """Missing associated documentation comment in .proto file."""
        context.set_code(grpc.StatusCode.UNIMPLEMENTED)
        context.set_details('Method not implemented!')
        raise NotImplementedError('Method not implemented!')


def add_ImageStreamServicer_to_server(servicer, server):
    rpc_method_handlers = {
        'SendImage': grpc.stream_stream_rpc_method_handler(
            servicer.SendImage,
            request_deserializer=image__pb2.ImageRequest.FromString,
            response_serializer=image__pb2.ImageResponse.SerializeToString,
        ),
    }
    generic_handler = grpc.method_handlers_generic_handler(
        'image.ImageStream', rpc_method_handlers)
    server.add_generic_rpc_handlers((generic_handler,))

 # This class is part of an EXPERIMENTAL API.


class ImageStream(object):
    """Missing associated documentation comment in .proto file."""

    @staticmethod
    def SendImage(request_iterator,
                  target,
                  options=(),
                  channel_credentials=None,
                  call_credentials=None,
                  insecure=False,
                  compression=None,
                  wait_for_ready=None,
                  timeout=None,
                  metadata=None):
        return grpc.experimental.stream_stream(request_iterator, target, '/image.ImageStream/SendImage',
                                               image__pb2.ImageRequest.SerializeToString,
                                               image__pb2.ImageResponse.FromString,
                                               options, channel_credentials,
                                               insecure, call_credentials, compression, wait_for_ready, timeout, metadata)
