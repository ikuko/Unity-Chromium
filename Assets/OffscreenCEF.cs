using System;
using System.Collections;
using Aleab.CefUnity.Structs;
using UnityEngine;
using Xilium.CefGlue;

namespace Aleab.CefUnity
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class OffscreenCEF : MonoBehaviour, IDisposable
    {
        [Space]
        [SerializeField]
        private Size windowSize = new Size(1280, 720);

        [SerializeField]
        private string url = "http://www.google.com";

        [Space]
        [SerializeField]
        private bool hideScrollbars = false;

        private bool shouldQuit = false;
        private OffscreenCEFClient cefClient;

        [Space]
        private CefBrowser cefBrowser;

        public Texture2D BrowserTexture { get; private set; }
        public bool IsDisposed { get; private set; }

        private void Awake()
        {
            this.BrowserTexture = new Texture2D(this.windowSize.Width, this.windowSize.Height, TextureFormat.BGRA32, false);
            this.GetComponent<MeshRenderer>().material.mainTexture = this.BrowserTexture;
        }

        void LateUpdate()
        {
            InstantiateCube();
        }

        void InstantiateCube()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Vector3 screenPos = Input.mousePosition;
                screenPos.z = 1000f;
                var target = Camera.main.ScreenToWorldPoint(screenPos);

                Debug.Log(target);
                Debug.Log("Click!");
                // this.cefClient.OnMouseDownEvent((int) target.x, (int) target.y);
                this.cefBrowser.GetMainFrame().ExecuteJavaScript(String.Format("document.elementFromPoint({0}, {1}).click();", target.x, target.y), "", 1);
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                Debug.Log("Wheel!");
                this.cefClient.OnMouseDownEvent(100, 100);
            }
        }

        private IEnumerator Start()
        {
            this.StartCef();
            this.StartCoroutine(this.MessagePump());
            // DontDestroyOnLoad(this.gameObject.transform.root.gameObject);
            yield return null;
        }

        private void OnDestroy()
        {
            this.Quit();
        }

        private void OnApplicationQuit()
        {
            this.Quit();
        }

        private void StartCef()
        {
            CefRuntime.Load();

            var cefMainArgs = new CefMainArgs(new string[] { });
            var cefApp = new OffscreenCEFClient.OffscreenCEFApp();

            // This is where the code path diverges for child processes.
            if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero) != -1)
                Debug.LogError("Could not start the secondary process.");

            var cefSettings = new CefSettings
            {
                MultiThreadedMessageLoop = false,
                LogSeverity = CefLogSeverity.Verbose,
                LogFile = "cef.log",
                WindowlessRenderingEnabled = true,
                NoSandbox = true,
            };

            // Start the browser process (a child process).
            CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, IntPtr.Zero);

            // Instruct CEF to not render to a window.
            CefWindowInfo cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.SetAsWindowless(IntPtr.Zero, false);

            // Settings for the browser window itself (e.g. enable JavaScript?).
            CefBrowserSettings cefBrowserSettings = new CefBrowserSettings()
            {
                BackgroundColor = new CefColor(255, 60, 85, 115),
                JavaScript = CefState.Enabled,
                JavaScriptAccessClipboard = CefState.Disabled,
                JavaScriptCloseWindows = CefState.Disabled,
                JavaScriptDomPaste = CefState.Disabled,
                LocalStorage = CefState.Disabled,
            };

            // Initialize some of the custom interactions with the browser process.
            this.cefClient = new OffscreenCEFClient(this.windowSize, this.hideScrollbars);

            // Start up the browser instance.
            cefBrowser = CefBrowserHost.CreateBrowserSync(cefWindowInfo, this.cefClient, cefBrowserSettings, this.url);
        }

        private void Quit()
        {
            this.cefBrowser.GetMainFrame().Dispose();
            this.shouldQuit = true;
            this.StopAllCoroutines();
            this.cefClient.Shutdown();
            CefRuntime.Shutdown();
            Dispose(false);
        }

        private IEnumerator MessagePump()
        {
            while (!this.shouldQuit)
            {
                CefRuntime.DoMessageLoopWork();
                if (!this.shouldQuit)
                    this.cefClient.UpdateTexture(this.BrowserTexture);
                yield return null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    //perform cleanup here
                }

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}