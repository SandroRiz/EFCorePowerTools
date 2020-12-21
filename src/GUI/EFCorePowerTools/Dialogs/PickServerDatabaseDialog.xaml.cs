﻿namespace EFCorePowerTools.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using Contracts.ViewModels;
    using Contracts.Views;
    using Shared.DAL;
    using Shared.Models;
    using ReverseEngineer20.ReverseEngineer;

    public partial class PickServerDatabaseDialog : IPickServerDatabaseDialog
    {
        private readonly Func<(DatabaseConnectionModel Connection, DatabaseDefinitionModel Definition, bool IncludeViews, bool FilterSchemas, SchemaInfo[] Schemas)> _getDialogResult;
        private readonly Action<IEnumerable<DatabaseConnectionModel>> _addConnections;
        private readonly Action<IEnumerable<DatabaseDefinitionModel>> _addDefinitions;
        private readonly Action<IEnumerable<SchemaInfo>> _addSchemas;

        public PickServerDatabaseDialog(ITelemetryAccess telemetryAccess,
                                        IPickServerDatabaseViewModel viewModel)
        {
            telemetryAccess.TrackPageView(nameof(PickServerDatabaseDialog));

            DataContext = viewModel;
            viewModel.CloseRequested += (sender, args) =>
            {
                DialogResult = args.DialogResult;
                Close();
            };
            _getDialogResult = () => (viewModel.SelectedDatabaseConnection, viewModel.SelectedDatabaseDefinition, viewModel.IncludeViews, viewModel.FilterSchemas, viewModel.Schemas.ToArray());
            _addConnections = models =>
            {
                foreach (var model in models)
                    viewModel.DatabaseConnections.Add(model);
            };
            _addDefinitions = models =>
            {
                foreach (var model in models)
                    viewModel.DatabaseDefinitions.Add(model);
            };
            _addSchemas = models =>
            {
                foreach (var model in models)
                    viewModel.Schemas.Add(model);
            };

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DatabaseConnectionCombobox.Focus();
        }

        public (bool ClosedByOK, (DatabaseConnectionModel Connection, DatabaseDefinitionModel Definition, bool IncludeViews, bool FilterSchemas, SchemaInfo[] Schemas) Payload) ShowAndAwaitUserResponse(bool modal)
        {
            bool closedByOkay;

            if (modal)
            {
                closedByOkay = ShowModal() == true;
            }
            else
            {
                closedByOkay = ShowDialog() == true;
            }

            return (closedByOkay, _getDialogResult());
        }

        void IPickServerDatabaseDialog.PublishConnections(IEnumerable<DatabaseConnectionModel> connections)
        {
            _addConnections(connections);
        }

        void IPickServerDatabaseDialog.PublishDefinitions(IEnumerable<DatabaseDefinitionModel> definitions)
        {
            _addDefinitions(definitions);
        }

        public void PublishSchemas(IEnumerable<SchemaInfo> schemas)
        {
            _addSchemas(schemas);
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            var connectionDialog = new ConnectionDialog();
            connectionDialog.ShowModal();
        }
    }
}