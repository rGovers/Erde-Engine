using Erde.Graphics.Variables;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Erde.Graphics
{
    public class Camera : GameObject, IDisposable
    {
        static LinkedList<Camera> m_cameras;
        static Camera             m_mainCamera;

        Matrix4                   m_projection;

        ClearBufferMask           m_clearFlags;

        Rectangle                 m_viewport;

        bool                      m_skybox;

        IPost                     m_post;

        float                     m_fov;

        int                       m_width;
        int                       m_height;

        float                     m_near;
        float                     m_far;

        RenderTexture             m_renderTexture;

        public static LinkedList<Camera> CameraList
        {
            get
            {
                return m_cameras;
            }
        }

        public static Camera MainCamera
        {
            get
            {
                return m_mainCamera;
            }
            set
            {
                m_mainCamera = value;
            }
        }

        public bool DrawSkybox
        {
            get
            {
                return m_skybox;
            }
            set
            {
                m_skybox = value;
            }
        }

        public RenderTexture RenderTexture
        {
            get
            {
                return m_renderTexture;
            }
        }

        public float Far
        {
            get
            {
                return m_far;
            }
        }

        public float Near
        {
            get
            {
                return m_near;
            }
        }

        public int Width
        {
            get
            {
                return m_width;
            }
        }

        public int Height
        {
            get
            {
                return m_width;
            }
        }

        public float FOV
        {
            get
            {
                return m_fov;
            }
        }

        public Matrix4 Projection
        {
            get
            {
                return m_projection;
            }
        }

        public Rectangle Viewport
        {
            get
            {
                return m_viewport;
            }
        }

        public ClearBufferMask ClearFlags
        {
            get
            {
                return m_clearFlags;
            }
            set
            {
                m_clearFlags = value;
            }
        }

        public IPost PostProcessing
        {
            get
            {
                return m_post;
            }
        }

        public void SetProjectionViewport (float a_fov, int a_width, int a_height, float a_near, float a_far)
        {
            m_fov = a_fov;

            // There is a crash if the resolution drops below 640 x 480
            m_width = Math.Max(a_width, 640);
            m_height = Math.Max(a_height, 480);

            m_near = a_near;
            m_far = a_far;

            m_projection = Matrix4.CreatePerspectiveFieldOfView(m_fov, m_width / (float)m_height, m_near, m_far);
            m_viewport = new Rectangle(0, 0, m_width, m_height);
        }

        public Camera (float a_fov, int a_width, int a_height, float a_near, float a_far, ClearBufferMask a_clearFlags = ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit, RenderTexture a_renderTexture = null, IPost a_post = null) : base()
        {
            SetProjectionViewport(a_fov, a_width, a_height, a_near, a_far);

            m_clearFlags = a_clearFlags;

            m_renderTexture = a_renderTexture;
            m_post = a_post;

            if (m_cameras == null)
            {
                m_cameras = new LinkedList<Camera>();
            }

            lock (m_cameras)
            {
                m_cameras.AddLast(this);

                if (m_mainCamera == null)
                {
                    m_mainCamera = this;
                }
            }
        }

        public Vector3 ScreenToWorld (Vector3 a_screenPos)
        {
            // This does not look right
            Matrix4 projInv = Projection.Inverted();

            Vector4 clipPos = new Vector4(a_screenPos.X * 2 - 1, a_screenPos.Y * 2 - 1, 0.0f, 1.0f);
            Vector4 viewPos = clipPos * projInv;
            viewPos /= viewPos.W;
            Vector4 worldPos = viewPos * Transform.ToMatrix();

            return worldPos.Xyz - Transform.Forward * a_screenPos.Z * Far;
        }

        private void Dispose (bool a_state)
        {
            Debug.Assert(a_state, string.Format("[Warning] Resource leaked {0}", GetType().ToString()));

            base.Dispose();

            lock (m_cameras)
            {
                if (m_mainCamera == this)
                {
                    m_mainCamera = null;
                }

                m_cameras.Remove(this);
            }
        }

        ~Camera ()
        {
            Dispose(false);
        }

        public new void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}