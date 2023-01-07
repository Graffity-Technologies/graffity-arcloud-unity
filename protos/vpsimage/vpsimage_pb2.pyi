from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from typing import ClassVar as _ClassVar, Mapping as _Mapping, Optional as _Optional, Union as _Union

DESCRIPTOR: _descriptor.FileDescriptor

class AvailableAreaRequest(_message.Message):
    __slots__ = ["gpsPosition", "maxDistance", "minDistance"]
    GPSPOSITION_FIELD_NUMBER: _ClassVar[int]
    MAXDISTANCE_FIELD_NUMBER: _ClassVar[int]
    MINDISTANCE_FIELD_NUMBER: _ClassVar[int]
    gpsPosition: Position
    maxDistance: int
    minDistance: int
    def __init__(self, gpsPosition: _Optional[_Union[Position, _Mapping]] = ..., maxDistance: _Optional[int] = ..., minDistance: _Optional[int] = ...) -> None: ...

class AvailableAreaResponse(_message.Message):
    __slots__ = ["gpsPosition", "isAvailable", "message"]
    GPSPOSITION_FIELD_NUMBER: _ClassVar[int]
    ISAVAILABLE_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    gpsPosition: Position
    isAvailable: bool
    message: str
    def __init__(self, message: _Optional[str] = ..., isAvailable: bool = ..., gpsPosition: _Optional[_Union[Position, _Mapping]] = ...) -> None: ...

class CameraInfo(_message.Message):
    __slots__ = ["pixelFocalLength", "principalPointX", "principalPointY", "radialDistortion"]
    PIXELFOCALLENGTH_FIELD_NUMBER: _ClassVar[int]
    PRINCIPALPOINTX_FIELD_NUMBER: _ClassVar[int]
    PRINCIPALPOINTY_FIELD_NUMBER: _ClassVar[int]
    RADIALDISTORTION_FIELD_NUMBER: _ClassVar[int]
    pixelFocalLength: float
    principalPointX: float
    principalPointY: float
    radialDistortion: float
    def __init__(self, pixelFocalLength: _Optional[float] = ..., principalPointX: _Optional[float] = ..., principalPointY: _Optional[float] = ..., radialDistortion: _Optional[float] = ...) -> None: ...

class ColmapCoordinate(_message.Message):
    __slots__ = ["px", "py", "pz", "qw", "qx", "qy", "qz", "tx", "ty", "tz"]
    PX_FIELD_NUMBER: _ClassVar[int]
    PY_FIELD_NUMBER: _ClassVar[int]
    PZ_FIELD_NUMBER: _ClassVar[int]
    QW_FIELD_NUMBER: _ClassVar[int]
    QX_FIELD_NUMBER: _ClassVar[int]
    QY_FIELD_NUMBER: _ClassVar[int]
    QZ_FIELD_NUMBER: _ClassVar[int]
    TX_FIELD_NUMBER: _ClassVar[int]
    TY_FIELD_NUMBER: _ClassVar[int]
    TZ_FIELD_NUMBER: _ClassVar[int]
    px: float
    py: float
    pz: float
    qw: float
    qx: float
    qy: float
    qz: float
    tx: float
    ty: float
    tz: float
    def __init__(self, qw: _Optional[float] = ..., qx: _Optional[float] = ..., qy: _Optional[float] = ..., qz: _Optional[float] = ..., tx: _Optional[float] = ..., ty: _Optional[float] = ..., tz: _Optional[float] = ..., px: _Optional[float] = ..., py: _Optional[float] = ..., pz: _Optional[float] = ...) -> None: ...

class Position(_message.Message):
    __slots__ = ["altitude", "latitude", "longitude"]
    ALTITUDE_FIELD_NUMBER: _ClassVar[int]
    LATITUDE_FIELD_NUMBER: _ClassVar[int]
    LONGITUDE_FIELD_NUMBER: _ClassVar[int]
    altitude: float
    latitude: float
    longitude: float
    def __init__(self, latitude: _Optional[float] = ..., longitude: _Optional[float] = ..., altitude: _Optional[float] = ...) -> None: ...

class VpsImageRequest(_message.Message):
    __slots__ = ["bytesImage", "cameraInfo", "gpsPosition", "message"]
    BYTESIMAGE_FIELD_NUMBER: _ClassVar[int]
    CAMERAINFO_FIELD_NUMBER: _ClassVar[int]
    GPSPOSITION_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    bytesImage: bytes
    cameraInfo: CameraInfo
    gpsPosition: Position
    message: str
    def __init__(self, message: _Optional[str] = ..., bytesImage: _Optional[bytes] = ..., gpsPosition: _Optional[_Union[Position, _Mapping]] = ..., cameraInfo: _Optional[_Union[CameraInfo, _Mapping]] = ...) -> None: ...

class VpsImageResponse(_message.Message):
    __slots__ = ["accuracy", "colmapCoor", "message", "worldCoor"]
    ACCURACY_FIELD_NUMBER: _ClassVar[int]
    COLMAPCOOR_FIELD_NUMBER: _ClassVar[int]
    MESSAGE_FIELD_NUMBER: _ClassVar[int]
    WORLDCOOR_FIELD_NUMBER: _ClassVar[int]
    accuracy: float
    colmapCoor: ColmapCoordinate
    message: str
    worldCoor: WorldCoordinate
    def __init__(self, message: _Optional[str] = ..., accuracy: _Optional[float] = ..., worldCoor: _Optional[_Union[WorldCoordinate, _Mapping]] = ..., colmapCoor: _Optional[_Union[ColmapCoordinate, _Mapping]] = ...) -> None: ...

class WorldCoordinate(_message.Message):
    __slots__ = ["altitude", "latitude", "longitude", "utm_x", "utm_y"]
    ALTITUDE_FIELD_NUMBER: _ClassVar[int]
    LATITUDE_FIELD_NUMBER: _ClassVar[int]
    LONGITUDE_FIELD_NUMBER: _ClassVar[int]
    UTM_X_FIELD_NUMBER: _ClassVar[int]
    UTM_Y_FIELD_NUMBER: _ClassVar[int]
    altitude: float
    latitude: float
    longitude: float
    utm_x: float
    utm_y: float
    def __init__(self, latitude: _Optional[float] = ..., longitude: _Optional[float] = ..., altitude: _Optional[float] = ..., utm_x: _Optional[float] = ..., utm_y: _Optional[float] = ...) -> None: ...
