﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClientInterfaces.State;
using ClientServices.UserInterface.Components;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;
using Label = ClientServices.UserInterface.Components.Label;

namespace ClientServices.State.States
{
    public class OptionsMenu : State, IState
    {
        #region Fields

        private readonly Label _btnApply;
        private readonly Sprite _background;

        private readonly Checkbox _chkFullscreen;
        private readonly Checkbox _chkVsync;

        private readonly Label _lblFullscreen;
        private readonly Label _lblVsync;
        private readonly Label _btnMainMenu;
        private readonly Listbox _lstResolution;
        private readonly Sprite _ticketBg;

        private readonly Dictionary<string, VideoMode> vmList = new Dictionary<string, VideoMode>();

        #endregion

        #region Properties

        #endregion

        public OptionsMenu(IDictionary<Type, object> managers)
            : base(managers)
        {
            _background = ResourceManager.GetSprite("mainbg");
            _background.Smoothing = Smoothing.Smooth;

            _lblFullscreen = new Label("Fullscreen", "CALIBRI", ResourceManager);

            _chkFullscreen = new Checkbox(ResourceManager);
            _chkFullscreen.Value = ConfigurationManager.GetFullscreen();
            _chkFullscreen.ValueChanged += _chkfullscreen_ValueChanged;

            _lblVsync = new Label("Vsync", "CALIBRI", ResourceManager);

            _chkVsync = new Checkbox(ResourceManager);
            _chkVsync.Value = ConfigurationManager.GetVsync();
            _chkVsync.ValueChanged += _chkvsync_ValueChanged;

            _lstResolution = new Listbox(250, 150, ResourceManager);
            _lstResolution.ItemSelected += _reslistbox_ItemSelected;

            IOrderedEnumerable<VideoMode> modes = from v in Gorgon.CurrentDriver.VideoModes
                                                  where
                                                      (v.Height > 748 && v.Width > 1024) &&
                                                      v.Format == BackBufferFormats.BufferRGB888 && v.RefreshRate >= 59
                                                  //GOSH I HOPE NOONES USING 16 BIT COLORS. OR RUNNING AT LESS THAN 59 hz
                                                  orderby v.Height*v.Width ascending
                                                  select v;

            if (!modes.Any())
                //No compatible videomodes at all. It is likely the game is being run on a calculator. TODO handle this.
                Application.Exit();

            foreach (VideoMode vm in modes)
            {
                if (!vmList.ContainsKey(GetVmString(vm)))
                {
                    vmList.Add(GetVmString(vm), vm);
                    _lstResolution.AddItem(GetVmString(vm));
                }
            }

            if (
                vmList.Any(
                    x =>
                    x.Value.Width == Gorgon.CurrentVideoMode.Width && x.Value.Height == Gorgon.CurrentVideoMode.Height &&
                    x.Value.RefreshRate ==
                    (Gorgon.Screen.Windowed ? Gorgon.DesktopVideoMode.RefreshRate : Gorgon.CurrentVideoMode.RefreshRate)))
            {
                KeyValuePair<string, VideoMode> curr =
                    vmList.FirstOrDefault(
                        x =>
                        x.Value.Width == Gorgon.CurrentVideoMode.Width &&
                        x.Value.Height == Gorgon.CurrentVideoMode.Height &&
                        x.Value.RefreshRate ==
                        (Gorgon.Screen.Windowed
                             ? Gorgon.DesktopVideoMode.RefreshRate
                             : Gorgon.CurrentVideoMode.RefreshRate));
                _lstResolution.SelectItem(curr.Key, false);
            }
            else
            {
                //No match due to different refresh rate in windowed mode. Just pick first resolution based on size only.
                KeyValuePair<string, VideoMode> curr =
                    vmList.FirstOrDefault(
                        x =>
                        x.Value.Width == Gorgon.CurrentVideoMode.Width &&
                        x.Value.Height == Gorgon.CurrentVideoMode.Height);
                _lstResolution.SelectItem(curr.Key, false);
            }

            _ticketBg = ResourceManager.GetSprite("ticketoverlay");

            _btnMainMenu = new Label("Main Menu", "CALIBRI", ResourceManager);
            _btnMainMenu.DrawBorder = true;
            _btnMainMenu.Clicked += _mainmenubtt_Clicked;

            _btnApply = new Label("Apply", "CALIBRI", ResourceManager);
            _btnApply.DrawBorder = true;
            _btnApply.Clicked += _applybtt_Clicked;


			_lstResolution.Position = new Point(45, (int)(Gorgon.CurrentClippingViewport.Height / 2.5f));
			_lstResolution.Update(0);
			_chkFullscreen.Position = new Point(_lstResolution.Position.X,
												_lstResolution.Position.Y + _lstResolution.ClientArea.Height + 10);
			_chkFullscreen.Update(0);
			_chkVsync.Position = new Point(_chkFullscreen.Position.X,
										   _chkFullscreen.Position.Y + _chkFullscreen.ClientArea.Height + 10);
			_chkVsync.Update(0);
			_lblFullscreen.Position = new Point(_chkFullscreen.Position.X + _chkFullscreen.ClientArea.Width + 3,
												_chkFullscreen.Position.Y + (int)(_chkFullscreen.ClientArea.Height / 2f) -
												(int)(_lblFullscreen.ClientArea.Height / 2f));
			_lblFullscreen.Update(0);
			_lblVsync.Position = new Point(_chkVsync.Position.X + _chkVsync.ClientArea.Width + 3,
										   _chkVsync.Position.Y + (int)(_chkVsync.ClientArea.Height / 2f) -
										   (int)(_chkVsync.ClientArea.Height / 2f));
			_lblVsync.Update(0);
			_btnMainMenu.Position = new Point(_lstResolution.Position.X + 650, _lstResolution.Position.Y);
			_btnMainMenu.Update(0);
			_btnApply.Position = new Point(_btnMainMenu.Position.X,
										   _btnMainMenu.Position.Y + _btnMainMenu.ClientArea.Height + 5);
			_btnApply.Update(0);
        }

        #region IState Members

        public void GorgonRender(FrameEventArgs e)
        {
            _background.Draw(new Rectangle(0, 0, Gorgon.CurrentClippingViewport.Width,
                                           Gorgon.CurrentClippingViewport.Height));
            _ticketBg.Draw(new Rectangle(0, (int) (Gorgon.CurrentClippingViewport.Height/2f - _ticketBg.Height/2f),
                                         (int) _ticketBg.Width, (int) _ticketBg.Height));
            UserInterfaceManager.Render();
        }

        public void FormResize()
        {
        }

        #endregion

        #region Input

        public void KeyDown(KeyboardInputEventArgs e)
        {
            UserInterfaceManager.KeyDown(e);
        }

        public void KeyUp(KeyboardInputEventArgs e)
        {
        }

        public void MouseUp(MouseInputEventArgs e)
        {
            UserInterfaceManager.MouseUp(e);
        }

        public void MouseDown(MouseInputEventArgs e)
        {
            UserInterfaceManager.MouseDown(e);
        }

        public void MouseMove(MouseInputEventArgs e)
        {
            UserInterfaceManager.MouseMove(e);
        }

        public void MouseWheelMove(MouseInputEventArgs e)
        {
            UserInterfaceManager.MouseWheelMove(e);
        }

        #endregion

        private void _chkvsync_ValueChanged(bool newValue, Checkbox sender)
        {
            ConfigurationManager.SetVsync(newValue);
        }

        private void _applybtt_Clicked(Label sender, MouseInputEventArgs e)
        {
            ApplyVideoMode();
        }

        private void _chkfullscreen_ValueChanged(bool newValue, Checkbox sender)
        {
            ConfigurationManager.SetFullscreen(newValue);
        }

        private void ApplyVideoMode()
        {
            Form owner = Gorgon.Screen.OwnerForm;
            Gorgon.Stop();

            Gorgon.SetMode(owner, (int) ConfigurationManager.GetDisplayWidth(),
                           (int) ConfigurationManager.GetDisplayHeight(), BackBufferFormats.BufferRGB888,
                           !ConfigurationManager.GetFullscreen(), false, false,
                           (int) ConfigurationManager.GetDisplayRefresh(),
                           (ConfigurationManager.GetVsync() ? VSyncIntervals.IntervalOne : VSyncIntervals.IntervalNone));

            if (!ConfigurationManager.GetFullscreen())
            {
                //Gee thanks gorgon for changing this stuff only when switching TO fullscreen.
                owner.FormBorderStyle = FormBorderStyle.Sizable;
                owner.WindowState = FormWindowState.Normal;
                owner.ControlBox = true;
                owner.MaximizeBox = true;
                owner.MinimizeBox = true;
            }

            Gorgon.Go();
        }

        private void _reslistbox_ItemSelected(Label item, Listbox sender)
        {
            if (vmList.ContainsKey(item.Text.Text))
            {
                VideoMode sel = vmList[item.Text.Text];
                ConfigurationManager.SetResolution((uint) sel.Width, (uint) sel.Height);
                ConfigurationManager.SetDisplayRefresh((uint) sel.RefreshRate);
            }
        }

        private string GetVmString(VideoMode vm)
        {
            return vm.Width.ToString() + "x" + vm.Height.ToString() + " @ " + vm.RefreshRate + " hz";
        }

        private void _exitbtt_Clicked(Label sender)
        {
            Environment.Exit(0);
        }

        private void _mainmenubtt_Clicked(Label sender, MouseInputEventArgs e)
        {
            StateManager.RequestStateChange<MainScreen>();
        }

        private void _connectbtt_Clicked(Label sender, MouseInputEventArgs e)
        {
        }

        private void _connecttxt_OnSubmit(string text)
        {
        }

        #region Startup, Shutdown, Update

        public void Startup()
        {
            NetworkManager.Disconnect();
            UserInterfaceManager.AddComponent(_btnMainMenu);
            UserInterfaceManager.AddComponent(_lstResolution);
            UserInterfaceManager.AddComponent(_chkFullscreen);
            UserInterfaceManager.AddComponent(_chkVsync);
            UserInterfaceManager.AddComponent(_lblFullscreen);
            UserInterfaceManager.AddComponent(_lblVsync);
            UserInterfaceManager.AddComponent(_btnApply);
        }


        public void Shutdown()
        {
            UserInterfaceManager.RemoveComponent(_btnMainMenu);
            UserInterfaceManager.RemoveComponent(_lstResolution);
            UserInterfaceManager.RemoveComponent(_chkFullscreen);
            UserInterfaceManager.RemoveComponent(_chkVsync);
            UserInterfaceManager.RemoveComponent(_lblFullscreen);
            UserInterfaceManager.RemoveComponent(_lblVsync);
            UserInterfaceManager.RemoveComponent(_btnApply);
        }

        public void Update(FrameEventArgs e)
        {
            _chkFullscreen.Value = ConfigurationManager.GetFullscreen();
            UserInterfaceManager.Update(e.FrameDeltaTime);
        }

        #endregion
    }
}