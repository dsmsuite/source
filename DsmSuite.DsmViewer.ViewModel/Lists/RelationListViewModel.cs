﻿using System.Collections.Generic;
using DsmSuite.DsmViewer.ViewModel.Common;
using System.Windows.Input;
using System.Windows;
using System.Text;
using DsmSuite.DsmViewer.Model.Interfaces;
using DsmSuite.DsmViewer.Application.Interfaces;
using System.Collections.ObjectModel;
using DsmSuite.DsmViewer.ViewModel.Editing.Relation;
using System;

namespace DsmSuite.DsmViewer.ViewModel.Lists
{
    public class RelationListViewModel : ViewModelBase
    {
        private IDsmApplication _application;
        private IDsmElement _selectedConsumer;
        private IDsmElement _selectedProvider;

        public event EventHandler<RelationEditViewModel> RelationAddStarted;
        public event EventHandler<RelationEditViewModel> RelationEditStarted;

        public RelationListViewModel(RelationsListViewModelType viewModelType, IDsmApplication application, IDsmElement selectedConsumer, IDsmElement selectedProvider)
        {
            _application = application;
            _selectedConsumer = selectedConsumer;
            _selectedProvider = selectedProvider;

            Title = "Relation List";
            IEnumerable<IDsmRelation> relations;
            switch (viewModelType)
            {
                case RelationsListViewModelType.ElementIngoingRelations:
                    SubTitle = $"Ingoing relations of {_selectedProvider.Fullname}";
                    relations = _application.FindIngoingRelations(_selectedProvider);
                    break;
                case RelationsListViewModelType.ElementOutgoingRelations:
                    SubTitle = $"Outgoing relations of {_selectedProvider.Fullname}";
                    relations = _application.FindOutgoingRelations(_selectedProvider);
                    break;
                case RelationsListViewModelType.ElementInternalRelations:
                    SubTitle = $"Internal relations of {_selectedProvider.Fullname}";
                    relations = _application.FindInternalRelations(_selectedProvider);
                    break;
                case RelationsListViewModelType.ConsumerProviderRelations:
                    SubTitle = $"Relations between consumer {_selectedConsumer.Fullname} and provider {_selectedProvider.Fullname}";
                    relations = _application.FindResolvedRelations(_selectedConsumer, _selectedProvider);
                    break;
                default:
                    SubTitle = "";
                    relations = new List<IDsmRelation>();
                    break;
            }

            List<RelationListItemViewModel> relationViewModels = new List<RelationListItemViewModel>();

            foreach (IDsmRelation relation in relations)
            {
                relationViewModels.Add(new RelationListItemViewModel(relation));
            }

            relationViewModels.Sort();

            int index = 1;
            foreach (RelationListItemViewModel viewModel in relationViewModels)
            {
                viewModel.Index = index;
                index++;
            }

            Relations = new ObservableCollection<RelationListItemViewModel>(relationViewModels);

            CopyToClipboardCommand = new RelayCommand<object>(CopyToClipboardExecute);
            DeleteRelationCommand = new RelayCommand<object>(DeleteRelationExecute, DeleteRelationCanExecute);
            EditRelationCommand = new RelayCommand<object>(EditRelationExecute, EditRelationCanExecute);
            AddRelationCommand = new RelayCommand<object>(AddRelationExecute, AddRelationCanExecute);
        }

        public string Title { get; }
        public string SubTitle { get; }

        public ObservableCollection<RelationListItemViewModel> Relations { get; }

        public RelationListItemViewModel SelectedRelation { get; set; }

        public ICommand CopyToClipboardCommand { get; }

        public ICommand DeleteRelationCommand { get; }
        public ICommand EditRelationCommand { get; }
        public ICommand AddRelationCommand { get; }

        private void CopyToClipboardExecute(object parameter)
        {
            StringBuilder builder = new StringBuilder();
            foreach (RelationListItemViewModel viewModel in Relations)
            {
                builder.AppendLine($"{viewModel.Index,-5}, {viewModel.ConsumerName,-100}, {viewModel.ProviderName,-100}, {viewModel.RelationType,-30}, {viewModel.RelationWeight,-10}, {viewModel.Properties,-150}");
            }
            Clipboard.SetText(builder.ToString());
        }

        private void DeleteRelationExecute(object parameter)
        {
            _application.DeleteRelation(SelectedRelation.Relation);
            Relations.Remove(SelectedRelation);
        }

        private bool DeleteRelationCanExecute(object parameter)
        {
            return SelectedRelation != null;
        }

        private void EditRelationExecute(object parameter)
        {
            RelationEditViewModel relationEditViewModel = new RelationEditViewModel(_application, SelectedRelation.Relation, null, null);
            RelationEditStarted?.Invoke(this, relationEditViewModel);
        }

        private bool EditRelationCanExecute(object parameter)
        {
            return SelectedRelation != null;
        }

        private void AddRelationExecute(object parameter)
        {
            RelationEditViewModel relationEditViewModel = new RelationEditViewModel(_application, null, null, null);
            RelationAddStarted?.Invoke(this, relationEditViewModel);
        }

        private bool AddRelationCanExecute(object parameter)
        {
            return true;
        }
    }
}
