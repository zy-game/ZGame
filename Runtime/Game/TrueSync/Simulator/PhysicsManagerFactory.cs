using UnityEngine;

namespace TrueSync
{
    /**
     *  @brief Manages physics simulation.
     **/
    public class PhysicsManager
    {
        /**
         *  @brief Indicates the type of physics simulations: 2D or 3D.
         **/
        public enum PhysicsType
        {
            W_2D,
            W_3D
        };

        /**
         *  @brief Returns a proper implementation of {@link IPhysicsManager}.
         **/
        public static IPhysicsManager instance;

        /**
         *  @brief Instantiates a new {@link IPhysicsManager}.
         *
         *  @param trueSyncConfig Indicates if is a 2D or 3D world.
         **/
        public static IPhysicsManager New(TrueSyncConfig trueSyncConfig)
        {
            if (trueSyncConfig.physics3DEnabled)
            {
                instance = new Physics3DSimulator();
                instance.Gravity = trueSyncConfig.gravity3D;
                instance.SpeculativeContacts = trueSyncConfig.speculativeContacts3D;
            }
            else if (trueSyncConfig.physics2DEnabled)
            {
                instance = new Physics2DSimulator();
                instance.Gravity = new TSVector(trueSyncConfig.gravity2D.x, trueSyncConfig.gravity2D.y, 0);
                instance.SpeculativeContacts = trueSyncConfig.speculativeContacts2D;
            }

            return instance;
        }

        /**
         *  @brief Instantiates a 3D physics for tests purpose.
         **/
        internal static void InitTest3D()
        {
            instance = new Physics3DSimulator();
            instance.Gravity = new TSVector(0, -10, 0);
            instance.LockedTimeStep = 0.02f;
            instance.Init();
        }

        /**
         *  @brief Instantiates a 2D physics for tests purpose.
         **/
        internal static void InitTest2D()
        {
            instance = new Physics2DSimulator();
            instance.Gravity = new TSVector(0, -10, 0);
            instance.LockedTimeStep = 0.02f;
            instance.Init();
        }


        public static void InitializedGameObject(GameObject gameObject, TSVector position, TSQuaternion rotation)
        {
            if (instance == null)
            {
                return;
            }

            ICollider[] tsColliders = gameObject.GetComponentsInChildren<ICollider>();
            if (tsColliders != null)
            {
                for (int index = 0, length = tsColliders.Length; index < length; index++)
                {
                    PhysicsManager.instance.AddBody(tsColliders[index]);
                }
            }

            TSTransform rootTSTransform = gameObject.GetComponent<TSTransform>();
            if (rootTSTransform != null)
            {
                rootTSTransform.Initialize();

                rootTSTransform.position = position;
                rootTSTransform.rotation = rotation;
            }

            TSTransform[] tsTransforms = gameObject.GetComponentsInChildren<TSTransform>();
            if (tsTransforms != null)
            {
                for (int index = 0, length = tsTransforms.Length; index < length; index++)
                {
                    TSTransform tsTransform = tsTransforms[index];

                    if (tsTransform != rootTSTransform)
                    {
                        tsTransform.Initialize();
                    }
                }
            }

            TSTransform2D rootTSTransform2D = gameObject.GetComponent<TSTransform2D>();
            if (rootTSTransform2D != null)
            {
                rootTSTransform2D.Initialize();

                rootTSTransform2D.position = new TSVector2(position.x, position.y);
                rootTSTransform2D.rotation = rotation.ToQuaternion().eulerAngles.z;
            }

            TSTransform2D[] tsTransforms2D = gameObject.GetComponentsInChildren<TSTransform2D>();
            if (tsTransforms2D != null)
            {
                for (int index = 0, length = tsTransforms2D.Length; index < length; index++)
                {
                    TSTransform2D tsTransform2D = tsTransforms2D[index];

                    if (tsTransform2D != rootTSTransform2D)
                    {
                        tsTransform2D.Initialize();
                    }
                }
            }
        }
    }
}