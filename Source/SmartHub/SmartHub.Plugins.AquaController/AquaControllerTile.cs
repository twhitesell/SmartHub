﻿using SmartHub.Plugins.WebUI.Attributes;
using SmartHub.Plugins.WebUI.Tiles;
using System;

namespace SmartHub.Plugins.AquaController
{
    [Tile]
    public class AquaControllerTile : TileBase
    {
        public override void PopulateWebModel(TileWebModel tileWebModel, dynamic options)
        {
            try
            {
                tileWebModel.title = "Аква-контроллер";
                tileWebModel.url = "webapp/aquacontroller/module-main";
                tileWebModel.className = "btn-info th-tile-icon th-tile-icon-fa fa-tachometer";
                tileWebModel.content = Context.GetPlugin<AquaControllerPlugin>().BuildTileContent();
                tileWebModel.SignalRReceiveHandler = Context.GetPlugin<AquaControllerPlugin>().BuildSignalRReceiveHandler();
            }
            catch (Exception ex)
            {
                tileWebModel.content = ex.Message;
            }
        }
    }
}
