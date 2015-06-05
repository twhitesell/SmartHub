﻿
define(
	['common', 'lib', 'text!webapp/management/zone-editor.html'],
    function (common, lib, templates) {

        var layoutView = lib.marionette.LayoutView.extend({
            template: lib._.template(templates),
            onShow: function () {
                var viewModel = this.options.viewModel;
                createMultiSelector($("#msMonitors"), "monitor");
                createMultiSelector($("#msControllers"), "controller");
                //createMultiSelector($("#msGraphs"), "graph");

                kendo.bind($("#content"), this.options.viewModel);

                function createMultiSelector(selector, entity) {
                    return selector.kendoMultiSelect({
                        dataSource: {
                            transport: {
                                read: {
                                    url: function () { return document.location.origin + "/api/management/" + entity + "/list"; }
                                }
                            },
                        },
                        dataValueField: "Id",
                        dataTextField: "Name",
                        valuePrimitive: true,
                        autoClose: false,
                        filter: "contains",
                        placeholder: "Выберите элементы...",
                    }).data("kendoMultiSelect");
                }
            }
        });

        return {
            LayoutView: layoutView
        };
    });