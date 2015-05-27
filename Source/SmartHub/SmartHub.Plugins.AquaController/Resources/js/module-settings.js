﻿define(
	['app', 'webapp/aquacontroller/module-settings-model', 'webapp/aquacontroller/module-settings-view'],
	function (application, models, views) {
	    var module = {
	        setSensorsConfiguration: function () {
	            models.setSensorsConfiguration(models.ViewModel.SensorsConfiguration);
	        },
	        reload: function () {
	            models.ViewModel.update(function () {
	                var view = new views.LayoutView();
	                view.on('SensorsConfiguration:set', module.setSensorsConfiguration);

	                application.setContentView(view);

	                view.bindModel(models.ViewModel);
	            });
	        }
	    };

	    return {
	        start: function () {
	            module.reload();
	        }
	    };
	});