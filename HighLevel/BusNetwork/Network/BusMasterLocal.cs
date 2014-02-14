using MFE.Hardware;
using Microsoft.SPOT.Hardware;
using System;
using System.Collections;

namespace BusNetwork.Network
{
    public class BusMasterLocal : BusMaster
    {
        #region Fields
        private BusConfiguration busConfig;
        #endregion

        #region Constructor
        public BusMasterLocal(BusConfiguration busConfig)
            : base(0)
        {
            this.busConfig = busConfig;
        }
        #endregion

        #region Public methods
        public override byte GetBusModuleType(ushort busModuleAddress)
        {
            byte type = 255; // initially set "unknown" type

            //I2CDevice.Configuration config = new I2CDevice.Configuration(busModuleAddress, BusConfiguration.ClockRate);
            //if (!busConfig.Bus.TryGetRegister(config, BusConfiguration.Timeout, BusModule.CmdGetType, out type))
            //    type = 255; // set "unknown" type

            return type;
        }
        public override void GetBusModuleControlLines(BusModule busModule)
        {
            for (byte type = 0; type < BusModule.ControlLineTypesToRequest; type++)
            {
                byte[] count = new byte[1]; // up to 256 numbers for one type
                I2CDevice.Configuration config = new I2CDevice.Configuration(busModule.Address, BusConfiguration.ClockRate);
                if (!busConfig.Bus.TryGetRegisters(config, BusConfiguration.Timeout, BusModule.CmdGetControlLineCount, new byte[] { type }, count))
                    count[0] = 0;

                for (byte number = 0; number < count[0]; number++)
                {
                    ControlLine controlLine = new ControlLine(this, busModule, (ControlLineType)type, number);
                    busModule.ControlLines.Add(controlLine);

                    // query control line state:
                    GetControlLineState(controlLine);
                }
            }
        }
        public override void GetControlLineState(ControlLine controlLine)
        {
            byte[] data = new byte[2] { (byte)controlLine.Type, controlLine.Number };

            I2CDevice.Configuration config = new I2CDevice.Configuration(controlLine.BusModule.Address, BusConfiguration.ClockRate);
            if (!busConfig.Bus.TryGetRegisters(config, BusConfiguration.Timeout, BusModule.CmdGetControlLineState, data, controlLine.State))
                controlLine.ResetState();
        }
        public override void SetControlLineState(ControlLine controlLine, byte[] state)
        {
            byte[] data = new byte[state.Length + 2];
            data[0] = (byte)controlLine.Type;
            data[1] = controlLine.Number;
            Array.Copy(state, 0, data, 2, state.Length);

            I2CDevice.Configuration config = new I2CDevice.Configuration(controlLine.BusModule.Address, BusConfiguration.ClockRate);
            if (!busConfig.Bus.TrySetRegister(config, BusConfiguration.Timeout, BusModule.CmdSetControlLineState, data))
            {
            }
        }
        #endregion

        #region Private methods
        bool on = false;
        protected override void Scan()
        {
            ArrayList addressesAdded = new ArrayList();
            ArrayList addressesRemoved = new ArrayList();

            //// for test!!!
            BusModules.Clear();

            for (ushort address = 1; address <= 127; address++)
            {
                byte type = 255;

                I2CDevice.Configuration config = new I2CDevice.Configuration(address, BusConfiguration.ClockRate);
                if (busConfig.Bus.TryGetRegister(config, BusConfiguration.Timeout, BusModule.CmdGetType, out type)) // address is online
                {
                    BusModule busModule = this[address];

                    if (busModule == null) // no registered module with this address
                    {
                        busModule = new BusModule(address, type);

                        // query control lines count:
                        GetBusModuleControlLines(busModule);

                        addressesAdded.Add(address);
                        BusModules.Add(busModule);

                        //// for test!!!
                        //on = !on;
                        //SetControlLineState((ControlLine)busModule.ControlLines[3], new byte[] { (byte)(on ? 1 : 0), 0 });
                        //GetBusModuleControlLines(busModule);
                    }
                    else // module with this address is registered
                    {
                        // update it???
                    }
                }
                else // address is offline
                {
                    BusModule busModule = this[address];
                    if (busModule != null) // offline module
                    {
                        addressesRemoved.Add(address);
                        BusModules.Remove(busModule);
                    }
                }
            }

            NotifyBusModulesCollectionChanged(addressesAdded, addressesRemoved);
        }
        #endregion
    }
}
