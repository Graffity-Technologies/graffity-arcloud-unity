using System;
using System.Runtime.Serialization;
using UnityEngine;
using Vpsimage;
using Solver;

namespace UnityEngine.Graffity.ARCloud
{
    internal struct PosePair
    {
        internal Pose AR;
        internal Pose VPS;

        public PosePair(Pose ar, Pose vps)
        {
            AR = ar;
            VPS = vps;
        }
    }

    public struct Pose
    {
        public Pose(Vector3 position,
                    Quaternion rotation,
                    Vector3 translation,
                    Google.Protobuf.Collections.RepeatedField<double> covariance, // ICollection<double> covariance = new ICollection(),
                    string timestamp = "",
                    float accuracy = 0.0f,
                    string id = "")
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Translation = translation;
            this.Timestamp = timestamp;
            this.Accuracy = accuracy;
            this.Covariance = covariance;
            this.Id = id;
        }

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Translation;
        public string Timestamp;
        public float Accuracy;
        public Google.Protobuf.Collections.RepeatedField<double> Covariance;
        public string Id;

        public Coordinate ToCoordinate()
        {
            var coor = new Coordinate()
            {
                Position = this.Position.ToVec3(),
                Rotation = this.Rotation.ToVec4(),
                Translation = this.Translation.ToVec3(),
                Covariance = { this.Covariance },
                Timestamp = this.Timestamp,
                Accuracy = this.Accuracy,
                ImageId = this.Id
            };
            return coor;
        }
    }

    public struct PositionGps
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;

        public static explicit operator Position(PositionGps positionGps) => new Position()
        {
            Latitude = positionGps.Latitude,
            Longitude = positionGps.Longitude,
            Altitude = positionGps.Altitude
        };
    }

    internal struct SolveTransformation
    {
        public Vector3 Translation;
        public Vector3 Scale;
        public Quaternion Rotation;
    }

    public class ARCloudException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ARCloudException()
        {
        }

        public ARCloudException(string message) : base(message)
        {
        }

        public ARCloudException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ARCloudException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    public class ARCloudExceptionNotAvailable : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ARCloudExceptionNotAvailable()
        {
        }

        public ARCloudExceptionNotAvailable(string message) : base(message)
        {
        }

        public ARCloudExceptionNotAvailable(string message, Exception inner) : base(message, inner)
        {
        }

        protected ARCloudExceptionNotAvailable(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}