using System.Collections.Generic;
using System.Linq;
using DsmSuite.DsmViewer.Model.Interfaces;

namespace DsmSuite.DsmViewer.Application.Queries
{
    /// <summary>
    /// Contains some but not all queries the application executes on the model
    /// </summary>
    /// TODO Can this class be made more useful?
    /// DsmApplicaton does some queries as well and dispatches others here. Can we make this class more useful
    /// by forwarding once and dispatching here?
    public class DsmQueries
    {
        private readonly IDsmModel _model;
        public DsmQueries(IDsmModel model)
        {
            _model = model;
        }

        public IEnumerable<IDsmElement> GetRelationConsumers(IDsmElement consumer, IDsmElement provider)
        {
            var relations = _model.FindRelations(consumer, provider)
                .OrderBy(x => x.Consumer.Fullname)
                .GroupBy(x => x.Consumer.Fullname)
                .Select(x => x.FirstOrDefault())
                .ToList();

            var elements = relations.Select(x => x.Consumer)
                .ToList();
            return elements;
        }

        public IEnumerable<IDsmElement> GetElementProvidedElements(IDsmElement element)
        {
            var relations = _model.FindIngoingRelations(element)
                .OrderBy(x => x.Provider.Fullname)
                .GroupBy(x => x.Provider.Fullname)
                .Select(x => x.FirstOrDefault())
                .ToList();

            var elements = relations.Select(x => x.Provider)
                .ToList();
            return elements;
        }

        public IEnumerable<IDsmElement> GetElementProviders(IDsmElement element)
        {
            var relations = _model.FindOutgoingRelations(element)
                .OrderBy(x => x.Provider.Fullname)
                .GroupBy(x => x.Provider.Fullname)
                .Select(x => x.FirstOrDefault())
                .ToList();

            var elements = relations.Select(x => x.Provider)
                .ToList();
            return elements;
        }

        public IEnumerable<IDsmElement> GetElementConsumers(IDsmElement element)
        {
            var relations = _model.FindIngoingRelations(element)
                .OrderBy(x => x.Consumer.Fullname)
                .GroupBy(x => x.Consumer.Fullname)
                .Select(x => x.FirstOrDefault())
                .ToList();

            var elements = relations.Select(x => x.Consumer)
                .ToList();
            return elements;
        }

        public IEnumerable<IDsmRelation> FindRelations(IDsmElement consumer, IDsmElement provider)
        {
            var relations = _model.FindRelations(consumer, provider)
                .OrderBy(x => x.Provider.Fullname)
                .ThenBy(x => x.Consumer.Fullname)
                .ToList();
            return relations;
        }

        public IEnumerable<IDsmElement> GetRelationProviders(IDsmElement consumer, IDsmElement provider)
        {
            var relations = _model.FindRelations(consumer, provider)
                .OrderBy(x => x.Provider.Fullname)
                .GroupBy(x => x.Provider.Fullname)
                .Select(x => x.FirstOrDefault())
                .ToList();

            var elements = relations.Select(x => x.Provider)
                .ToList();
            return elements;
        }

        public IEnumerable<IDsmRelation> FindIngoingRelations(IDsmElement element)
        {
            return _model.FindIngoingRelations(element);
        }

        public IEnumerable<IDsmRelation> FindOutgoingRelations(IDsmElement element)
        {
            return _model.FindOutgoingRelations(element);
        }

        public IEnumerable<IDsmRelation> FindInternalRelations(IDsmElement element)
        {
            return _model.FindInternalRelations(element);
        }

        public IEnumerable<IDsmRelation> FindExternalRelations(IDsmElement element)
        {
            return _model.FindExternalRelations(element);
        }
    }
}
