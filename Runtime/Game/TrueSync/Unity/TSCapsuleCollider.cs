using UnityEngine;
using UnityEngine.Serialization;
using TrueSync.Physics3D;

namespace TrueSync {
    /**
     *  @brief Collider with a capsule shape. 
     **/
    [AddComponentMenu("TrueSync/Physics/CapsuleCollider", 0)]
    public class TSCapsuleCollider : TSCollider {

        [FormerlySerializedAs("radius")]
        [SerializeField]
        private FP _radius;

        /**
         *  @brief Radius of the capsule. 
         **/
        public FP radius {
            get {
                if (body != null) {
                    return ((CapsuleShape)body.Shape).Radius;
                }

                return _radius;
            }
            set {
                _radius = value;

                if (body != null) {
                    ((CapsuleShape)body.Shape).Radius = _radius;
                }
            }
        }

        [FormerlySerializedAs("length")]
        [SerializeField]
        private FP _length;

        /**
         *  @brief Length of the capsule. 
         **/
        public FP length {
            get {
                if (body != null) {
                    return ((CapsuleShape)body.Shape).Length;
                }

                return _length;
            }
            set {
                _length = value;

                if (body != null) {
                    ((CapsuleShape)body.Shape).Length = _length;
                }
            }
        }

        /**
         *  @brief Create the internal shape used to represent a TSCapsuleCollider.
         **/
        public override Shape CreateShape() {
            return new CapsuleShape(length, radius);
        }

        protected override void DrawGizmos() {
            Gizmos.DrawWireSphere(Vector3.zero, 1);
            Gizmos.DrawWireSphere(new TSVector(0, length / radius - 2 * radius, 0).ToVector(), 1);
            Gizmos.DrawWireSphere(new TSVector(0, -length / radius + 2 * radius, 0).ToVector(), 1);
        }

        protected override Vector3 GetGizmosSize() {
            return Vector3.one * radius.AsFloat();
        }

    }
}