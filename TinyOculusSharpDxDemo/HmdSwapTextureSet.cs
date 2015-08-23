using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TinyOculusSharpDxDemo
{
    public class HmdSwapTextureSet : IDisposable
    {
        private Size m_resolution;
        public Size Resolution
        {
            get
            {
                return m_resolution;
            }
        }

        private List<Texture2D> m_textureResList;
        public List<Texture2D> Textures
        {
            get
            {
                return m_textureResList;
            }
        }

        private List<RenderTargetView> m_renderTargetViewList;
        public List<RenderTargetView> RenderTargetView
        {
            get
            {
                return m_renderTargetViewList;
            }
        }

        public IntPtr Ptr
        {
            get
            {
                return m_textureSet.Ptr;
            }
        }

        public int CurrentIndex
        {
            get
            {
                return m_textureSet.Value.CurrentIndex;
            }
        }

        public HmdSwapTextureSet(IntPtr hmdPtr, Device device, int width, int height)
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
                BindFlags = (uint)(BindFlags.RenderTarget | BindFlags.ShaderResource),
                CPUAccessFlags = (uint)CpuAccessFlags.None,
                MiscFlags = (uint)ResourceOptionFlags.None,
            };

            unsafe
            {
                IntPtr textureSetPtr;
                int result = LibOVR.ovrHmd_CreateSwapTextureSetD3D11(hmdPtr, device.NativePointer, (IntPtr)(&cDesc), out textureSetPtr);
                if (result != 0)
                {
                    MessageBox.Show("Failed to ovrHmd_CreateSwapTexturesetD3D11() code=" + result);
                    return;
                }

                var textureSet = CRef<LibOVR.ovrSwapTextureSet>.FromPtr(textureSetPtr);
                if (textureSet == null)
                {
                    return;
                }

                var textureList = new List<CRef<LibOVR.ovrTexture>>();
                for (int texIndex = 0; texIndex < textureSet.Value.TextureCount; ++texIndex)
                {
                    IntPtr texPtr = textureSet.Value.Textures + sizeof(LibOVR.ovrTexture) * texIndex;
                    var tex = CRef<LibOVR.ovrTexture>.FromPtr(texPtr);
                    textureList.Add(tex);
                }

                m_hmdPtr = hmdPtr;
                m_textureSet = textureSet;
                m_textures = textureList.ToArray();
            }

            var csize = m_textures[0].Value.Header.TextureSize;
            m_resolution = new Size(csize.w, csize.h);

            // make SharpDx resource
            m_textureResList = m_textures.Select(t => new Texture2D(t.Value.Texture)).ToList();

            // make and set texture views
            m_renderTargetViewList = m_textureResList.Select(t => new RenderTargetView(device, t)).ToList();
            _SetTextureView(0, m_renderTargetViewList[0]);
            _SetTextureView(1, m_renderTargetViewList[1]);
        }

        public void Dispose()
        {
            foreach (var renderTargetView in m_renderTargetViewList)
            {
                renderTargetView.Dispose();
            }
            m_renderTargetViewList.Clear();

            LibOVR.ovrHmd_DestroySwapTextureSet(m_hmdPtr, m_textureSet.Ptr);
            m_textureSet.Clear();
            m_textures = null;
        }

        public void AdvanceToNextTexture()
        {
            m_textureSet.Value.CurrentIndex = (m_textureSet.Value.CurrentIndex + 1) % m_textureSet.Value.TextureCount;
            unsafe
            {
                *(LibOVR.ovrSwapTextureSet*)(m_textureSet.Ptr) = m_textureSet.Value;
            }
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

        #region private members

        private IntPtr m_hmdPtr;
        private CRef<LibOVR.ovrSwapTextureSet> m_textureSet;
        private CRef<LibOVR.ovrTexture>[] m_textures;

        #endregion // private members

        #region private mthods

        public void _SetTextureView(int index, RenderTargetView textureView)
        {
            m_textures[index].Value.View = textureView.NativePointer;
            unsafe
            {
                *(LibOVR.ovrTexture*)(m_textures[index].Ptr) = m_textures[index].Value;
            }
        }

        #endregion // private methods
    }
}
