using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
    public class HmdMirrorTexture : IDisposable
    {

        public HmdMirrorTexture(IntPtr hmdPtr, Device device, int width, int height)
        {
            var cDesc = new _D3D11_TEXTURE2D_DESC()
            {
                Width = (uint)width,
                Height = (uint)height,
                MipLevels = (uint)1,
                ArraySize = (uint)1,
                Format = (uint)Format.R8G8B8A8_UNorm,
                SampleDesc_Count = (uint)1,
                SampleDesc_Quality = (uint)0,
                Usage = (uint)ResourceUsage.Default,
                BindFlags = (uint)BindFlags.None,
                CPUAccessFlags = (uint)CpuAccessFlags.None,
                MiscFlags = (uint)ResourceOptionFlags.None,
            };

            unsafe
            {
                IntPtr mirrorTexturePtr;
                int result = LibOVR.ovrHmd_CreateMirrorTextureD3D11(hmdPtr, device.NativePointer, (IntPtr)(&cDesc), out mirrorTexturePtr);
                if (result != 0)
                {
                    MessageBox.Show("Failed to ovrHmd_CreateMirrorTextureD3D11() code=" + result);
                    return;
                }

                IntPtr nativeTexturePtr = ((LibOVR.ovrTexture*)mirrorTexturePtr)->Texture;
                var texture = new Texture2D(nativeTexturePtr);

                // succeeded all
                m_mirrorTexturePtr = mirrorTexturePtr;
                m_hmdPtr = hmdPtr;
                m_texture = texture;
            }
        }

        public void Dispose()
        {
            if (m_mirrorTexturePtr != IntPtr.Zero)
            {
                LibOVR.ovrHmd_DestroyMirrorTexture(m_hmdPtr, m_mirrorTexturePtr);
                m_mirrorTexturePtr = IntPtr.Zero;
                m_hmdPtr = IntPtr.Zero;
                m_texture = null;
            }
        }

        public Texture2D GetResource()
        {
            return m_texture;
        }

        #region private types

        [StructLayout(LayoutKind.Sequential)]
        struct _D3D11_TEXTURE2D_DESC
        {
            public uint Width;
            public uint Height;
            public uint MipLevels;
            public uint ArraySize;
            public uint Format;
            public uint SampleDesc_Count;
            public uint SampleDesc_Quality;
            public uint Usage;
            public uint BindFlags;
            public uint CPUAccessFlags;
            public uint MiscFlags;
        };

        #endregion // private types

        private IntPtr m_hmdPtr = IntPtr.Zero;
        private IntPtr m_mirrorTexturePtr = IntPtr.Zero;
        private Texture2D m_texture = null;
    }
}
