# -*- coding: utf-8 -*-
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: image.proto
"""Generated protocol buffer code."""
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from google.protobuf import reflection as _reflection
from google.protobuf import symbol_database as _symbol_database
# @@protoc_insertion_point(imports)

_sym_db = _symbol_database.Default()


DESCRIPTOR = _descriptor.FileDescriptor(
    name='image.proto',
    package='image',
    syntax='proto3',
    serialized_options=None,
    create_key=_descriptor._internal_create_key,
    serialized_pb=b'\n\x0bimage.proto\x12\x05image\"\x80\x01\n\x0cImageRequest\x12\x0f\n\x07message\x18\x01 \x01(\t\x12\x12\n\nbytesImage\x18\x02 \x01(\x0c\x12$\n\x0bgpsPosition\x18\x03 \x01(\x0b\x32\x0f.image.Position\x12%\n\ncameraInfo\x18\x04 \x01(\x0b\x32\x11.image.CameraInfo\"\x8a\x01\n\rImageResponse\x12\x0f\n\x07message\x18\x01 \x01(\t\x12\x10\n\x08\x61\x63\x63uracy\x18\x02 \x01(\x02\x12)\n\tworldCoor\x18\x03 \x01(\x0b\x32\x16.image.WorldCoordinate\x12+\n\ncolmapCoor\x18\x04 \x01(\x0b\x32\x17.image.ColmapCoordinate\"A\n\x08Position\x12\x10\n\x08latitude\x18\x01 \x01(\x01\x12\x11\n\tlongitude\x18\x02 \x01(\x01\x12\x10\n\x08\x61ltitude\x18\x03 \x01(\x01\"f\n\x0fWorldCoordinate\x12\x10\n\x08latitude\x18\x01 \x01(\x01\x12\x11\n\tlongitude\x18\x02 \x01(\x01\x12\x10\n\x08\x61ltitude\x18\x03 \x01(\x01\x12\r\n\x05utm_x\x18\x04 \x01(\x01\x12\r\n\x05utm_y\x18\x05 \x01(\x01\"\x8a\x01\n\x10\x43olmapCoordinate\x12\n\n\x02qw\x18\x01 \x01(\x01\x12\n\n\x02qx\x18\x02 \x01(\x01\x12\n\n\x02qy\x18\x03 \x01(\x01\x12\n\n\x02qz\x18\x04 \x01(\x01\x12\n\n\x02tx\x18\x05 \x01(\x01\x12\n\n\x02ty\x18\x06 \x01(\x01\x12\n\n\x02tz\x18\x07 \x01(\x01\x12\n\n\x02px\x18\x08 \x01(\x01\x12\n\n\x02py\x18\t \x01(\x01\x12\n\n\x02pz\x18\n \x01(\x01\"r\n\nCameraInfo\x12\x18\n\x10pixelFocalLength\x18\x01 \x01(\x02\x12\x17\n\x0fprincipalPointX\x18\x02 \x01(\x02\x12\x17\n\x0fprincipalPointY\x18\x03 \x01(\x02\x12\x18\n\x10radialDistortion\x18\x04 \x01(\x02\x32\x45\n\x05Image\x12<\n\tSendImage\x12\x13.image.ImageRequest\x1a\x14.image.ImageResponse\"\x00(\x01\x30\x01\x62\x06proto3'
)


_IMAGEREQUEST = _descriptor.Descriptor(
    name='ImageRequest',
    full_name='image.ImageRequest',
    filename=None,
    file=DESCRIPTOR,
    containing_type=None,
    create_key=_descriptor._internal_create_key,
    fields=[
        _descriptor.FieldDescriptor(
            name='message', full_name='image.ImageRequest.message', index=0,
            number=1, type=9, cpp_type=9, label=1,
            has_default_value=False, default_value=b"".decode('utf-8'),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='bytesImage', full_name='image.ImageRequest.bytesImage', index=1,
            number=2, type=12, cpp_type=9, label=1,
            has_default_value=False, default_value=b"",
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='gpsPosition', full_name='image.ImageRequest.gpsPosition', index=2,
            number=3, type=11, cpp_type=10, label=1,
            has_default_value=False, default_value=None,
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='cameraInfo', full_name='image.ImageRequest.cameraInfo', index=3,
            number=4, type=11, cpp_type=10, label=1,
            has_default_value=False, default_value=None,
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
    ],
    extensions=[
    ],
    nested_types=[],
    enum_types=[
    ],
    serialized_options=None,
    is_extendable=False,
    syntax='proto3',
    extension_ranges=[],
    oneofs=[
    ],
    serialized_start=23,
    serialized_end=151,
)


_IMAGERESPONSE = _descriptor.Descriptor(
    name='ImageResponse',
    full_name='image.ImageResponse',
    filename=None,
    file=DESCRIPTOR,
    containing_type=None,
    create_key=_descriptor._internal_create_key,
    fields=[
        _descriptor.FieldDescriptor(
            name='message', full_name='image.ImageResponse.message', index=0,
            number=1, type=9, cpp_type=9, label=1,
            has_default_value=False, default_value=b"".decode('utf-8'),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='accuracy', full_name='image.ImageResponse.accuracy', index=1,
            number=2, type=2, cpp_type=6, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='worldCoor', full_name='image.ImageResponse.worldCoor', index=2,
            number=3, type=11, cpp_type=10, label=1,
            has_default_value=False, default_value=None,
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='colmapCoor', full_name='image.ImageResponse.colmapCoor', index=3,
            number=4, type=11, cpp_type=10, label=1,
            has_default_value=False, default_value=None,
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
    ],
    extensions=[
    ],
    nested_types=[],
    enum_types=[
    ],
    serialized_options=None,
    is_extendable=False,
    syntax='proto3',
    extension_ranges=[],
    oneofs=[
    ],
    serialized_start=154,
    serialized_end=292,
)


_POSITION = _descriptor.Descriptor(
    name='Position',
    full_name='image.Position',
    filename=None,
    file=DESCRIPTOR,
    containing_type=None,
    create_key=_descriptor._internal_create_key,
    fields=[
        _descriptor.FieldDescriptor(
            name='latitude', full_name='image.Position.latitude', index=0,
            number=1, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='longitude', full_name='image.Position.longitude', index=1,
            number=2, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='altitude', full_name='image.Position.altitude', index=2,
            number=3, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
    ],
    extensions=[
    ],
    nested_types=[],
    enum_types=[
    ],
    serialized_options=None,
    is_extendable=False,
    syntax='proto3',
    extension_ranges=[],
    oneofs=[
    ],
    serialized_start=294,
    serialized_end=359,
)


_WORLDCOORDINATE = _descriptor.Descriptor(
    name='WorldCoordinate',
    full_name='image.WorldCoordinate',
    filename=None,
    file=DESCRIPTOR,
    containing_type=None,
    create_key=_descriptor._internal_create_key,
    fields=[
        _descriptor.FieldDescriptor(
            name='latitude', full_name='image.WorldCoordinate.latitude', index=0,
            number=1, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='longitude', full_name='image.WorldCoordinate.longitude', index=1,
            number=2, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='altitude', full_name='image.WorldCoordinate.altitude', index=2,
            number=3, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='utm_x', full_name='image.WorldCoordinate.utm_x', index=3,
            number=4, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='utm_y', full_name='image.WorldCoordinate.utm_y', index=4,
            number=5, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
    ],
    extensions=[
    ],
    nested_types=[],
    enum_types=[
    ],
    serialized_options=None,
    is_extendable=False,
    syntax='proto3',
    extension_ranges=[],
    oneofs=[
    ],
    serialized_start=361,
    serialized_end=463,
)


_COLMAPCOORDINATE = _descriptor.Descriptor(
    name='ColmapCoordinate',
    full_name='image.ColmapCoordinate',
    filename=None,
    file=DESCRIPTOR,
    containing_type=None,
    create_key=_descriptor._internal_create_key,
    fields=[
        _descriptor.FieldDescriptor(
            name='qw', full_name='image.ColmapCoordinate.qw', index=0,
            number=1, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='qx', full_name='image.ColmapCoordinate.qx', index=1,
            number=2, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='qy', full_name='image.ColmapCoordinate.qy', index=2,
            number=3, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='qz', full_name='image.ColmapCoordinate.qz', index=3,
            number=4, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='tx', full_name='image.ColmapCoordinate.tx', index=4,
            number=5, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='ty', full_name='image.ColmapCoordinate.ty', index=5,
            number=6, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='tz', full_name='image.ColmapCoordinate.tz', index=6,
            number=7, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='px', full_name='image.ColmapCoordinate.px', index=7,
            number=8, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='py', full_name='image.ColmapCoordinate.py', index=8,
            number=9, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='pz', full_name='image.ColmapCoordinate.pz', index=9,
            number=10, type=1, cpp_type=5, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
    ],
    extensions=[
    ],
    nested_types=[],
    enum_types=[
    ],
    serialized_options=None,
    is_extendable=False,
    syntax='proto3',
    extension_ranges=[],
    oneofs=[
    ],
    serialized_start=466,
    serialized_end=604,
)


_CAMERAINFO = _descriptor.Descriptor(
    name='CameraInfo',
    full_name='image.CameraInfo',
    filename=None,
    file=DESCRIPTOR,
    containing_type=None,
    create_key=_descriptor._internal_create_key,
    fields=[
        _descriptor.FieldDescriptor(
            name='pixelFocalLength', full_name='image.CameraInfo.pixelFocalLength', index=0,
            number=1, type=2, cpp_type=6, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='principalPointX', full_name='image.CameraInfo.principalPointX', index=1,
            number=2, type=2, cpp_type=6, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='principalPointY', full_name='image.CameraInfo.principalPointY', index=2,
            number=3, type=2, cpp_type=6, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
        _descriptor.FieldDescriptor(
            name='radialDistortion', full_name='image.CameraInfo.radialDistortion', index=3,
            number=4, type=2, cpp_type=6, label=1,
            has_default_value=False, default_value=float(0),
            message_type=None, enum_type=None, containing_type=None,
            is_extension=False, extension_scope=None,
            serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
    ],
    extensions=[
    ],
    nested_types=[],
    enum_types=[
    ],
    serialized_options=None,
    is_extendable=False,
    syntax='proto3',
    extension_ranges=[],
    oneofs=[
    ],
    serialized_start=606,
    serialized_end=720,
)

_IMAGEREQUEST.fields_by_name['gpsPosition'].message_type = _POSITION
_IMAGEREQUEST.fields_by_name['cameraInfo'].message_type = _CAMERAINFO
_IMAGERESPONSE.fields_by_name['worldCoor'].message_type = _WORLDCOORDINATE
_IMAGERESPONSE.fields_by_name['colmapCoor'].message_type = _COLMAPCOORDINATE
DESCRIPTOR.message_types_by_name['ImageRequest'] = _IMAGEREQUEST
DESCRIPTOR.message_types_by_name['ImageResponse'] = _IMAGERESPONSE
DESCRIPTOR.message_types_by_name['Position'] = _POSITION
DESCRIPTOR.message_types_by_name['WorldCoordinate'] = _WORLDCOORDINATE
DESCRIPTOR.message_types_by_name['ColmapCoordinate'] = _COLMAPCOORDINATE
DESCRIPTOR.message_types_by_name['CameraInfo'] = _CAMERAINFO
_sym_db.RegisterFileDescriptor(DESCRIPTOR)

ImageRequest = _reflection.GeneratedProtocolMessageType('ImageRequest', (_message.Message,), {
    'DESCRIPTOR': _IMAGEREQUEST,
    '__module__': 'image_pb2'
    # @@protoc_insertion_point(class_scope:image.ImageRequest)
})
_sym_db.RegisterMessage(ImageRequest)

ImageResponse = _reflection.GeneratedProtocolMessageType('ImageResponse', (_message.Message,), {
    'DESCRIPTOR': _IMAGERESPONSE,
    '__module__': 'image_pb2'
    # @@protoc_insertion_point(class_scope:image.ImageResponse)
})
_sym_db.RegisterMessage(ImageResponse)

Position = _reflection.GeneratedProtocolMessageType('Position', (_message.Message,), {
    'DESCRIPTOR': _POSITION,
    '__module__': 'image_pb2'
    # @@protoc_insertion_point(class_scope:image.Position)
})
_sym_db.RegisterMessage(Position)

WorldCoordinate = _reflection.GeneratedProtocolMessageType('WorldCoordinate', (_message.Message,), {
    'DESCRIPTOR': _WORLDCOORDINATE,
    '__module__': 'image_pb2'
    # @@protoc_insertion_point(class_scope:image.WorldCoordinate)
})
_sym_db.RegisterMessage(WorldCoordinate)

ColmapCoordinate = _reflection.GeneratedProtocolMessageType('ColmapCoordinate', (_message.Message,), {
    'DESCRIPTOR': _COLMAPCOORDINATE,
    '__module__': 'image_pb2'
    # @@protoc_insertion_point(class_scope:image.ColmapCoordinate)
})
_sym_db.RegisterMessage(ColmapCoordinate)

CameraInfo = _reflection.GeneratedProtocolMessageType('CameraInfo', (_message.Message,), {
    'DESCRIPTOR': _CAMERAINFO,
    '__module__': 'image_pb2'
    # @@protoc_insertion_point(class_scope:image.CameraInfo)
})
_sym_db.RegisterMessage(CameraInfo)


_IMAGE = _descriptor.ServiceDescriptor(
    name='Image',
    full_name='image.Image',
    file=DESCRIPTOR,
    index=0,
    serialized_options=None,
    create_key=_descriptor._internal_create_key,
    serialized_start=722,
    serialized_end=791,
    methods=[
        _descriptor.MethodDescriptor(
            name='SendImage',
            full_name='image.Image.SendImage',
            index=0,
            containing_service=None,
            input_type=_IMAGEREQUEST,
            output_type=_IMAGERESPONSE,
            serialized_options=None,
            create_key=_descriptor._internal_create_key,
        ),
    ])
_sym_db.RegisterServiceDescriptor(_IMAGE)

DESCRIPTOR.services_by_name['Image'] = _IMAGE

# @@protoc_insertion_point(module_scope)
