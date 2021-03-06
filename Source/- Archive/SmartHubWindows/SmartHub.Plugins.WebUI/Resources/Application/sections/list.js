﻿define(
	['app', 'application/sections/list-model', 'application/sections/list-view'],
	function (application, models, views) {
	    var api = {
	        addSectionItemTile: function (childView) {
	            // childView (section item) properties:
	            /*
                    id = Guid.NewGuid(),
                    name = sectionItem.Title,
                    path = sectionItem.GetModulePath(),
                    sortOrder = sectionItem.SortOrder,
                    tileTypeFullName = sectionItem.TileTypeFullName
                */

	            // if tileTypeFullName is defined:
	            // parameters are empty,
	            // title & url must be populated in TileBase::PopulateWebModel method directly;

	            // if tileTypeFullName is undefined:
	            // the AppSectionTile type is used,
	            // parameters has "title" and "url" fields populated from "name" and "path" of section item;
	            // title & url are being populated in AppSectionTile::PopulateWebModel method from parameters "title" and "url";

	            var tileTypeFullName = childView.model.get('tileTypeFullName');
	            var parameters = { };

	            if (!tileTypeFullName) {
	                tileTypeFullName = 'SmartHub.Plugins.WebUI.Tiles.AppSectionTile';
	                parameters.title = childView.model.get('name');
	                parameters.url = childView.model.get('path');
	            }

	            application.addTile(tileTypeFullName, parameters);
	        },
	        navigate: function (childView) {
	            var path = childView.model.get('path');
	            application.navigate(path);
	        },
	        reload: function (requestName, pageTitle) {
	            // todo: переписать выбор метода для загрузки списка
	            models[requestName]().done(function (items) {
	                var view = new views.SectionListView({ collection: items, title: pageTitle });
	                view.on('childview:sections:add-tile', api.addSectionItemTile);
	                view.on('childview:sections:navigate', api.navigate);

	                application.setContentView(view);
	            });
	        }
	    };

	    return {
	        api: api
	    };
	});