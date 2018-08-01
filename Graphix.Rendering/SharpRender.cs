using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Direct3D11 = SharpDX.Direct3D11;
using Direct2D1 = SharpDX.Direct2D1;
using DXGI = SharpDX.DXGI;
using DirectWrite = SharpDX.DirectWrite;
using SharpDX.Windows;
using Color = System.Drawing.Color;

namespace Graphix.Rendering
{
    class SharpRender : IDisposable
    {
        RenderForm form;

        public Direct2D1.DeviceContext d2dContext;
        public DXGI.SwapChain swapChain;
        public DirectWrite.Factory wrFactory;

        Direct3D11.Device d3device;
        Direct3D11.Device1 defDevice;
        DXGI.Device2 dxgiDevice2;
        DXGI.Adapter dxgiAdapter;
        DXGI.Factory2 dxgiFactory2;
        Direct2D1.Device d2dDevice;
        Direct2D1.Factory fac;
        DXGI.Surface bb;
        Direct2D1.Bitmap1 target;

        public Direct2D1.SolidColorBrush brush;

    public SharpRender(RenderForm form)
        {
            this.form = form;
            var adapters = new DXGI.Factory1().Adapters;
            DXGI.Adapter myadapter = null;
            for (int i = 0; i < adapters.Length; ++i)
            {
                Logger.Log(string.Format("Adapter Found: [{0}] " +
                    "{1}\tDeviceId={5}" +
                    "{1}\tLuid={6}" +
                    "{1}\tVendorId={10}" +
                    "{1}\tSubsystemId={9}" +
                    "{1}\tDescription={4}" +
                    "{1}\tRevision={7}" +
                    "{1}\tDedicatedSystemMemory={2}" +
                    "{1}\tDedicatedVideoMemory={3}" +
                    "{1}\tSharedSystemMemory={8}" +
                    "",
                    i, Environment.NewLine,
                    adapters[i].Description.DedicatedSystemMemory,
                    adapters[i].Description.DedicatedVideoMemory, adapters[i].Description.Description,
                    adapters[i].Description.DeviceId, adapters[i].Description.Luid,
                    adapters[i].Description.Revision, adapters[i].Description.SharedSystemMemory,
                    adapters[i].Description.SubsystemId, adapters[i].Description.VendorId));
                var outputs = adapters[i].Outputs;
                for (int j = 0; j<outputs.Length; ++j)
                    Logger.Log(string.Format("Output Found: [{0},{1}]"+
                        "{2}\tDeviceName={4}" +
                        "{2}\tIsAttachedToDesktop={5}" +
                        "{2}\tMonitorHandle={6}" +
                        "{2}\tDesktopBounds={3}" +
                        "{2}\tRotation={7}" +
                        "",
                        i, j, Environment.NewLine,
                        (Rectangle)outputs[j].Description.DesktopBounds,
                        outputs[j].Description.DeviceName,
                        outputs[j].Description.IsAttachedToDesktop,
                        outputs[j].Description.MonitorHandle,
                        outputs[j].Description.Rotation));
                if (outputs.Length > 0 && myadapter == null)
                    myadapter = adapters[i];
            }
            d3device = new Direct3D11.Device(
                myadapter,
                Direct3D11.DeviceCreationFlags.BgraSupport);
                 //SharpDX.Direct3D.DriverType.Hardware,
                 //Direct3D11.DeviceCreationFlags.BgraSupport | 
                 //Direct3D11.DeviceCreationFlags.Debug);
             defDevice = d3device.QueryInterface<Direct3D11.Device1>();
             dxgiDevice2 = defDevice.QueryInterface<DXGI.Device2>();
             dxgiAdapter = dxgiDevice2.Adapter;
             dxgiFactory2 = dxgiAdapter.GetParent<DXGI.Factory2>();
             var scDescription = new DXGI.SwapChainDescription1()
             {
                 Width = 0,
                 Height = 0,
                 Format = DXGI.Format.B8G8R8A8_UNorm,
                 Stereo = false,
                 SampleDescription = new DXGI.SampleDescription(1, 0),
                 Usage = DXGI.Usage.RenderTargetOutput,
                 BufferCount = 2,
                 Scaling = DXGI.Scaling.None,
                 SwapEffect = DXGI.SwapEffect.FlipSequential
             };
             swapChain = new DXGI.SwapChain1(dxgiFactory2, defDevice, form.Handle,
                 ref scDescription, null, null);
             d2dDevice = new Direct2D1.Device(dxgiDevice2);
             d2dContext = new Direct2D1.DeviceContext(d2dDevice, 
                 Direct2D1.DeviceContextOptions.None);
             fac = new Direct2D1.Factory(Direct2D1.FactoryType.SingleThreaded);
             var dpi = fac.DesktopDpi;
             var bMProperties = new Direct2D1.BitmapProperties1(
                 new Direct2D1.PixelFormat(DXGI.Format.B8G8R8A8_UNorm,
                     Direct2D1.AlphaMode.Premultiplied),
                 dpi.Width, dpi.Height,
                 Direct2D1.BitmapOptions.CannotDraw | Direct2D1.BitmapOptions.Target);
             bb = swapChain.GetBackBuffer<DXGI.Surface>(0);
             target = new Direct2D1.Bitmap1(d2dContext, bb, bMProperties);
             d2dContext.Target = target;
             wrFactory = new DirectWrite.Factory();

             brush = new Direct2D1.SolidColorBrush(d2dContext, c(Color.White));
        }

        public static SharpDX.Mathematics.Interop.RawColor4 c(Color color)
        {
            const float mult = 1 / 255f;
            return new SharpDX.Mathematics.Interop.RawColor4(
                color.R * mult, color.G * mult, color.B * mult, color.A * mult);
        }

        public void Dispose()
        {
            brush.Dispose();

            wrFactory.Dispose();
            target.Dispose();
            bb.Dispose();
            fac.Dispose();
            d2dDevice.Dispose();
            dxgiFactory2.Dispose();
            dxgiAdapter.Dispose();
            dxgiDevice2.Dispose();
            defDevice.Dispose();
            d3device.Dispose();
            swapChain.Dispose();
            d2dContext.Dispose();
        }
    }
}
