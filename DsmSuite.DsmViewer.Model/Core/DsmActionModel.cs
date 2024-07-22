using System.Collections.Generic;
using DsmSuite.DsmViewer.Model.Interfaces;
using DsmSuite.DsmViewer.Model.Persistency;

namespace DsmSuite.DsmViewer.Model.Core
{
    /// <summary>
     /// Manages the actions of a model. Only used by DsmModel, which forwards calls here.
     /// </summary>
    public class DsmActionModel : IDsmActionModelFileCallback
    {
        private readonly List<IDsmAction> _actions;
        private int _lastActionId;

        public DsmActionModel()
        {
            _actions = new List<IDsmAction>();
            _lastActionId = 0;
        }

        public void Clear()
        {
            _actions.Clear();
            _lastActionId = 0;
        }

        public IDsmAction ImportAction(int id, string type, IReadOnlyDictionary<string, string> data)
        {
            if (id > _lastActionId)
            {
                _lastActionId = id;
            }

            IDsmAction action = new DsmAction(id, type, data);
            _actions.Add(action);
            return action;
        }

        public IDsmAction AddAction(string type, IReadOnlyDictionary<string, string> data)
        {
            _lastActionId++;
            IDsmAction action = new DsmAction(_lastActionId, type, data);
            _actions.Add(action);
            return action;
        }

        public IEnumerable<IDsmAction> GetExportedActions()
        {
            return _actions;
        }

        public int GetExportedActionCount()
        {
            return _actions.Count;
        }
    }
}
