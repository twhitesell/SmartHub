//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AquaExpert {
    using Gadgeteer;
    using GTM = Gadgeteer.Modules;
    
    
    public partial class Program : Gadgeteer.Program {
        
        /// <summary>The UsbClientDP module using socket 8 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.UsbClientDP usbClientDP;
        
        /// <summary>The MoistureSensor module using socket 13 of the mainboard.</summary>
        private Gadgeteer.Modules.Seeed.MoistureSensor moistureSensorUpper;
        
        /// <summary>The Relay module using socket 1 of the mainboard.</summary>
        private Gadgeteer.Modules.LoveElectronics.Relay relays;
        
        /// <summary>The MoistureSensor module (not connected).</summary>
        private Gadgeteer.Modules.Seeed.MoistureSensor moistureSensorLower;
        
        /// <summary>The WiFi_RS21 (Premium) module using socket 3 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.WiFi_RS21 wifi_RS21;
        
        /// <summary>The PHTemp module using socket 12 of the mainboard.</summary>
        private Gadgeteer.Modules.LoveElectronics.PHTemp pHTempSensor;
        
        /// <summary>The LED Strip module using socket 18 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.LED_Strip indicators;
        
        /// <summary>The SDCard module using socket 9 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.SDCard sdCard;
        
        /// <summary>This property provides access to the Mainboard API. This is normally not necessary for an end user program.</summary>
        protected new static GHIElectronics.Gadgeteer.FEZRaptor Mainboard {
            get {
                return ((GHIElectronics.Gadgeteer.FEZRaptor)(Gadgeteer.Program.Mainboard));
            }
            set {
                Gadgeteer.Program.Mainboard = value;
            }
        }
        
        /// <summary>This method runs automatically when the device is powered, and calls ProgramStarted.</summary>
        public static void Main() {
            // Important to initialize the Mainboard first
            Program.Mainboard = new GHIElectronics.Gadgeteer.FEZRaptor();
            Program p = new Program();
            p.InitializeModules();
            p.ProgramStarted();
            // Starts Dispatcher
            p.Run();
        }
        
        private void InitializeModules() {
            this.usbClientDP = new GTM.GHIElectronics.UsbClientDP(8);
            this.moistureSensorUpper = new GTM.Seeed.MoistureSensor(13);
            this.relays = new GTM.LoveElectronics.Relay(1);
            Microsoft.SPOT.Debug.Print("The module \'moistureSensorLower\' was not connected in the designer and will be nu" +
                    "ll.");
            this.wifi_RS21 = new GTM.GHIElectronics.WiFi_RS21(3);
            this.pHTempSensor = new GTM.LoveElectronics.PHTemp(12);
            this.indicators = new GTM.GHIElectronics.LED_Strip(18);
            this.sdCard = new GTM.GHIElectronics.SDCard(9);
        }
    }
}
