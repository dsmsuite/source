using DsmSuite.Common.Util;
using DsmSuite.DsmViewer.Model.Interfaces;
using System.Collections.Generic;
using System.Linq;
using DsmSuite.DsmViewer.Model.Persistency;

namespace DsmSuite.DsmViewer.Model.Core
{
    /// <summary>
    /// Manages the relations of a model. Only used by DsmModel, which forwards calls here.
    /// </summary>
    public class DsmRelationModel : IDsmRelationModelFileCallback
    {
        private readonly Dictionary<int /*relationId*/, DsmRelation> _relationsById;
        private readonly Dictionary<int /*relationId*/, DsmRelation> _deletedRelationsById;

        private int _lastRelationId;

        public DsmRelationModel()
        {
            _relationsById = new Dictionary<int, DsmRelation>();
            _deletedRelationsById = new Dictionary<int, DsmRelation>();
            _lastRelationId = 0;
        }

        public void Clear()
        {
            _relationsById.Clear();
            _deletedRelationsById.Clear();

            _lastRelationId = 0;
        }

        public void ClearHistory()
        {
            _deletedRelationsById.Clear();
        }

        public IDsmRelation ImportRelation(int relationId, IDsmElement consumer, IDsmElement provider, string type, int weight, IDictionary<string, string> properties, bool deleted)
        {
            DsmRelation relation = null;

            if ((consumer != null) && (provider != null))
            {
                Logger.LogDataModelMessage(
                    $"Import relation relationId={relationId} consumerId={consumer.Id} providerId={provider.Id} type={type} weight={weight}");

                if (relationId > _lastRelationId)
                {
                    _lastRelationId = relationId;
                }


                if (consumer.Id != provider.Id)
                {
                    relation = new DsmRelation(relationId, consumer, provider, type, weight, properties) {IsDeleted = deleted};
                    if (deleted)
                    {
                        UnregisterRelation(relation);
                    }
                    else
                    {
                        RegisterRelation(relation);
                    }
                }
            }

            return relation;
        }

        public IDsmRelation AddRelation(IDsmElement consumer, IDsmElement provider, string type, int weight, IDictionary<string, string> properties)
        {
            DsmRelation relation = null;

            if ((consumer != null) && (provider != null))
            {
                Logger.LogDataModelMessage(
                    $"Add relation consumerId={consumer.Id} providerId={provider.Id} type={type} weight={weight}");

                if (consumer.Id != provider.Id)
                {
                    _lastRelationId++;
                    relation = new DsmRelation(_lastRelationId, consumer, provider, type, weight, properties) {IsDeleted = false};
                    RegisterRelation(relation);
                }
            }

            return relation;
        }

        public void ChangeRelationType(IDsmRelation relation, string type)
        {
            DsmRelation changedRelation = relation as DsmRelation;
            if (changedRelation != null)
            {
                UnregisterRelation(changedRelation);

                changedRelation.Type = type;

                RegisterRelation(changedRelation);
            }
        }

        public IEnumerable<string> GetRelationTypes()
        {
            return DsmRelation.GetTypeNames();
        }

        public void ChangeRelationWeight(IDsmRelation relation, int weight)
        {
            DsmRelation changedRelation = relation as DsmRelation;
            if (changedRelation != null)
            {
                UnregisterRelation(changedRelation);

                changedRelation.Weight = weight;

                RegisterRelation(changedRelation);
            }
        }

        public void RemoveRelation(int relationId)
        {
            if (_relationsById.ContainsKey(relationId))
            {
                DsmRelation relation = _relationsById[relationId];
                if (relation != null)
                {
                    UnregisterRelation(relation);
                }
            }
        }

        public void UnremoveRelation(int relationId)
        {
            if (_deletedRelationsById.ContainsKey(relationId))
            {
                DsmRelation relation = _deletedRelationsById[relationId];
                if (relation != null)
                {
                    RegisterRelation(relation);
                }
            }
        }

        public int GetDependencyWeight(IDsmElement consumer, IDsmElement provider)
        {
            int weight = 0;
            DsmElement consumerElement = consumer as DsmElement;
            if (consumerElement != null)
            {
                weight = consumerElement.Dependencies.GetDerivedDependencyWeight(provider);
            }
            return weight;
        }

        public int GetDirectDependencyWeight(IDsmElement consumer, IDsmElement provider)
        {
            int weight = 0;
            DsmElement consumerElement = consumer as DsmElement;
            if (consumerElement != null)
            {
                weight = consumerElement.Dependencies.GetDirectDependencyWeight(provider);
            }
            return weight;
        }

        public CycleType IsCyclicDependency(IDsmElement consumer, IDsmElement provider)
        {
            if ((GetDirectDependencyWeight(consumer, provider) > 0) &&
                (GetDirectDependencyWeight(provider, consumer) > 0))
            {
                return CycleType.System;
            }
            else if ((GetDependencyWeight(consumer, provider) > 0) &&
                     (GetDependencyWeight(provider, consumer) > 0))
            {
                return CycleType.Hierarchical;
            }
            else
            {
                return CycleType.None;
            }
        }

        public DsmRelation GetRelationById(int id)
        {
            return _relationsById.ContainsKey(id) ? _relationsById[id] : null;
        }

        public DsmRelation GetDeletedRelationById(int id)
        {
            return _deletedRelationsById.ContainsKey(id) ? _deletedRelationsById[id] : null;
        }

        public IDsmRelation FindRelation(IDsmElement consumer, IDsmElement provider, string type)
        {
            DsmRelation foundRelation = null;
            foreach (DsmRelation relation in FindRelations(consumer, provider))
            {
                if (relation.Type == type)
                {
                    foundRelation = relation;
                }
            }
            return foundRelation;
        }

        public IEnumerable<DsmRelation> FindRelations(IDsmElement consumer, IDsmElement provider)
        {
            IList<DsmRelation> relations = new List<DsmRelation>();

            DsmElement consumerDsmElement = consumer as DsmElement;
            DsmElement providerDsmElement = provider as DsmElement;
            if (consumerDsmElement?.Dependencies != null && providerDsmElement?.Dependencies != null)
            {
                IDictionary<int, DsmElement> ps = providerDsmElement.GetElementAndItsChildren();
                IDictionary<int, DsmElement> cs = consumerDsmElement.GetElementAndItsChildren();

                foreach (DsmElement c in cs.Values)
                {
                    foreach (DsmElement p in ps.Values)
                    {
                        foreach (DsmRelation relation in c.Dependencies.GetOutgoingRelations(p))
                        {
                            if (!relation.IsDeleted)
                            {
                                relations.Add(relation);
                            }
                        }
                    }
                }
            }
            return relations;
        }

        public int GetRelationCount(IDsmElement consumer, IDsmElement provider)
        {
            int count =0;

            DsmElement consumerDsmElement = consumer as DsmElement;
            DsmElement providerDsmElement = provider as DsmElement;
            if (consumerDsmElement?.Dependencies != null && providerDsmElement?.Dependencies != null)
            {
                IDictionary<int, DsmElement> ps = providerDsmElement.GetElementAndItsChildren();
                IDictionary<int, DsmElement> cs = consumerDsmElement.GetElementAndItsChildren();

                foreach (DsmElement c in cs.Values)
                {
                    foreach (DsmElement p in ps.Values)
                    {
                        foreach (DsmRelation relation in c.Dependencies.GetOutgoingRelations(p))
                        {
                            if (!relation.IsDeleted)
                            {
                                count++;
                            }
                        }
                    }
                }
            }
            return count;
        }

        public IEnumerable<IDsmRelation> FindIngoingRelations(IDsmElement element)
        {
            return FindRelations(element, RelationDirection.Ingoing, RelationScope.External);
        }

        public IEnumerable<IDsmRelation> FindOutgoingRelations(IDsmElement element)
        {
            return FindRelations(element, RelationDirection.Outgoing, RelationScope.External);
        }

        public IEnumerable<IDsmRelation> FindInternalRelations(IDsmElement element)
        {
            return FindRelations(element, RelationDirection.Outgoing, RelationScope.Internal);
        }

        public IEnumerable<IDsmRelation> FindExternalRelations(IDsmElement element)
        {
            return FindRelations(element, RelationDirection.Both, RelationScope.External);
        }

        private IEnumerable<IDsmRelation> FindRelations(IDsmElement element, RelationDirection direction, RelationScope scope)
        {
            List<DsmRelation> relations = new List<DsmRelation>();

            DsmElement dsmElement = element as DsmElement;
            if (dsmElement?.Dependencies != null)
            {
                IDictionary<int, DsmElement> elements = dsmElement.GetElementAndItsChildren();

                foreach (DsmElement e in elements.Values)
                {
                    if (direction != RelationDirection.Ingoing)
                    {
                        foreach (DsmRelation relation in e.Dependencies.GetOutgoingRelations())
                        {
                            if (HasSelectedScope(scope, relation.Provider, elements) && !relation.IsDeleted)
                            {
                                relations.Add(relation);
                            }
                        }
                    }

                    if (direction != RelationDirection.Outgoing)
                    {
                        foreach (DsmRelation relation in e.Dependencies.GetIngoingRelations())
                        {
                            if (HasSelectedScope(scope, relation.Consumer, elements) && !relation.IsDeleted)
                            {
                                relations.Add(relation);
                            }
                        }
                    }
                }
            }
            return relations;
        }

        private bool HasSelectedScope(RelationScope scope, IDsmElement element, IDictionary<int, DsmElement> elements)
        {
            switch (scope)
            {
                case RelationScope.Internal:
                    return elements.ContainsKey(element.Id);
                case RelationScope.External:
                    return !elements.ContainsKey(element.Id);
                case RelationScope.Both:
                    return true;
                default:
                    return true;
            }
        }

        public int GetHierarchicalCycleCount(IDsmElement element)
        {
            int cyclesCount = 0;
            CountCycles(element, CycleType.Hierarchical, ref cyclesCount);
            return cyclesCount / 2;
        }

        public int GetSystemCycleCount(IDsmElement element)
        {
            int cyclesCount = 0;
            CountCycles(element, CycleType.System, ref cyclesCount);
            return cyclesCount / 2;
        }

        public IEnumerable<IDsmRelation> GetRelations()
        {
            return _relationsById.Values;
        }

        public int GetRelationCount()
        {
            return _relationsById.Values.Count;
        }

        public IEnumerable<IDsmRelation> GetExportedRelations()
        {
            List<IDsmRelation> exportedRelations = new List<IDsmRelation>();
            exportedRelations.AddRange(_relationsById.Values);
            exportedRelations.AddRange(_deletedRelationsById.Values);
            return exportedRelations.OrderBy(x => x.Id);
        }

        public int GetExportedRelationCount()
        {
            return _relationsById.Values.Count + _deletedRelationsById.Values.Count;
        }

        public void UnregisterElementRelations(IDsmElement element)
        {
            List<DsmRelation> toBeRelationsUnregistered = new List<DsmRelation>();

            foreach (DsmRelation relation in _relationsById.Values)
            {
                if ((element.Id == relation.Consumer.Id) ||
                    (element.Id == relation.Provider.Id))
                {
                    toBeRelationsUnregistered.Add(relation);
                }
            }

            foreach (DsmRelation relation in toBeRelationsUnregistered)
            {
                UnregisterRelation(relation);
            }
        }

        public void ReregisterElementRelations(IDsmElement element)
        {
            List<DsmRelation> toBeRelationsReregistered = new List<DsmRelation>();

            foreach (DsmRelation relation in _deletedRelationsById.Values)
            {
                if ((element.Id == relation.Consumer.Id) ||
                    (element.Id == relation.Provider.Id))
                {
                    toBeRelationsReregistered.Add(relation);
                }
            }

            foreach (DsmRelation relation in toBeRelationsReregistered)
            {
                RegisterRelation(relation);
            }
        }

        private void RegisterRelation(DsmRelation relation)
        {
            relation.IsDeleted = false;
            _relationsById[relation.Id] = relation;

            if (_deletedRelationsById.ContainsKey(relation.Id))
            {
                _deletedRelationsById.Remove(relation.Id);
            }

            DsmElement consumer = relation.Consumer as DsmElement;
            consumer.Dependencies.AddOutgoingRelation(relation);

            DsmElement provider = relation.Provider as DsmElement;
            provider.Dependencies.AddIngoingRelation(relation);

            AddWeights(relation);
        }

        private void UnregisterRelation(DsmRelation relation)
        {
            relation.IsDeleted = true;
            _relationsById.Remove(relation.Id);

            _deletedRelationsById[relation.Id] = relation;

            DsmElement consumer = relation.Consumer as DsmElement;
            consumer.Dependencies.RemoveOutgoingRelation(relation);

            DsmElement provider = relation.Provider as DsmElement;
            provider.Dependencies.RemoveIngoingRelation(relation);

            RemoveWeights(relation);
        }

        public void AddWeights(IDsmRelation relation)
        {
            DsmElement currentConsumer = relation.Consumer as DsmElement;
            while (currentConsumer != null)
            {
                IDsmElement currentProvider = relation.Provider;
                while (currentProvider != null)
                {
                    if ((currentConsumer.Id != currentProvider.Id) && 
                        !currentConsumer.IsRoot && 
                        !currentProvider.IsRoot &&
                        !currentConsumer.IsRecursiveChildOf(currentProvider) &&
                        !currentProvider.IsRecursiveChildOf(currentConsumer))
                    {
                        currentConsumer.Dependencies.AddDerivedWeight(currentProvider, relation.Weight);
                    }
                    currentProvider = currentProvider.Parent;
                }
                currentConsumer = currentConsumer.Parent as DsmElement;
            }
        }

        public void RemoveWeights(IDsmRelation relation)
        {
            DsmElement currentConsumer = relation.Consumer as DsmElement;
            while (currentConsumer != null)
            {
                IDsmElement currentProvider = relation.Provider;
                while (currentProvider != null)
                {
                    if ((currentConsumer.Id != currentProvider.Id) &&
                        !currentConsumer.IsRoot &&
                        !currentProvider.IsRoot &&
                        !currentConsumer.IsRecursiveChildOf(currentProvider) &&
                        !currentProvider.IsRecursiveChildOf(currentConsumer))
                    {
                        currentConsumer.Dependencies.RemoveDerivedWeight(currentProvider, relation.Weight);
                    }
                    currentProvider = currentProvider.Parent;
                }
                currentConsumer = currentConsumer.Parent as DsmElement;
            }
        }

        private void CountCycles(IDsmElement element, CycleType cycleType, ref int cycleCount)
        {
            foreach (IDsmElement consumer in element.Children)
            {
                foreach (IDsmElement provider in element.Children)
                {
                    if (IsCyclicDependency(consumer, provider) == cycleType)
                    {
                        cycleCount++;
                    }
                }
            }

            foreach (IDsmElement child in element.Children)
            {
                CountCycles(child, cycleType, ref cycleCount);
            }
        }
    }
}
