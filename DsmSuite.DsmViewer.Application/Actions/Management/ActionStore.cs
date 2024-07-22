using System;
using DsmSuite.DsmViewer.Application.Actions.Element;
using DsmSuite.DsmViewer.Application.Actions.Relation;
using DsmSuite.DsmViewer.Application.Actions.Snapshot;
using DsmSuite.DsmViewer.Application.Interfaces;
using DsmSuite.DsmViewer.Model.Interfaces;
using System.Collections.Generic;

namespace DsmSuite.DsmViewer.Application.Actions.Management
{
    /// <summary>
    /// Stores/restores user actions to/from the model.
    /// </summary>
    /// <remarks>
    /// The ActionType enum tags are used as the action string for the model.
    /// </remarks>
    public class ActionStore
    {
        private readonly IDsmModel _model;
        private readonly IActionManager _actionManager;
        // Translate ActionType's to the implementing classes.
        // The ActionType enum tags are used as the action string for the model.
        // TODO Isn't a string->Type dictionary more suitable?
        // TODO Make certain (compile/runtime) all actions are indeed in the table (cut/copy/paste currently aren't)
        // otherwise we can't load model that we saved.
        private readonly Dictionary<ActionType, Type> _types;

        public ActionStore(IDsmModel model, IActionManager actionManager)
        {
            _model = model;
            _actionManager = actionManager;
            _types = new Dictionary<ActionType, Type>();

            RegisterActionTypes();
        }

        /// <summary>
        /// Loads all actions from the model into the ActionManager.
        /// Unrecognized actions are silently ignored.
        /// If any action cannot be loaded correctly, the ActionManager is cleared.
        /// </summary>
        public void LoadFromModel()
        {
            foreach (IDsmAction action in _model.GetActions())
            {
                ActionType actionType;
                if (ActionType.TryParse(action.Type, out actionType))
                {
                    if (_types.ContainsKey(actionType))
                    {
                        Type type = _types[actionType];
                        object[] args = { _model, _actionManager.GetContext(), action.Data };
                        IAction instance = Activator.CreateInstance(type, args) as IAction;
 
                        if (instance != null)
                        {
                            _actionManager.Add(instance);
                        }
                    }
                }
            }

            if (!_actionManager.Validate())
            {
                _actionManager.Clear();
            }
        }

        /// <summary>
        /// Saves all actions in the ActionManager to the model.
        /// </summary>
        public void SaveToModel()
        {
            if (_actionManager.Validate())
            {
                foreach (IAction action in _actionManager.GetActionsInChronologicalOrder())
                {
                    _model.AddAction(action.Type.ToString(), action.Data);
                }
            }
        }

        private void RegisterActionTypes()
        {
            //TODO cut/copy/paste actions not present
            _types[ElementChangeNameAction.RegisteredType] = typeof(ElementChangeNameAction);
            _types[ElementChangeTypeAction.RegisteredType] = typeof(ElementChangeTypeAction);
            _types[ElementChangeParentAction.RegisteredType] = typeof(ElementChangeParentAction);
            _types[ElementCreateAction.RegisteredType] = typeof(ElementCreateAction);
            _types[ElementDeleteAction.RegisteredType] = typeof(ElementDeleteAction);
            _types[ElementMoveDownAction.RegisteredType] = typeof(ElementMoveDownAction);
            _types[ElementMoveUpAction.RegisteredType] = typeof(ElementMoveUpAction);
            _types[ElementSortAction.RegisteredType] = typeof(ElementSortAction);

            _types[RelationChangeTypeAction.RegisteredType] = typeof(RelationChangeTypeAction);
            _types[RelationChangeWeightAction.RegisteredType] = typeof(RelationChangeWeightAction);
            _types[RelationCreateAction.RegisteredType] = typeof(RelationCreateAction);
            _types[RelationDeleteAction.RegisteredType] = typeof(RelationDeleteAction);

            _types[MakeSnapshotAction.RegisteredType] = typeof(MakeSnapshotAction);
        }
    }
}
