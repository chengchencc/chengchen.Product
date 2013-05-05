using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;
using System.Reflection;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using BlackMamba.Framework.Core;

namespace BlackMamba.Framework.RedisMapper
{
    public class RedisService : IRedisService, IDisposable
    {

        #region Redis Client instance
        public virtual IRedisClientsManager RedisClientManager
        {
            get
            {
                return RedisClientManagerFactory.Instance.MixedClientManager;
            }
        }
        #endregion

        private string _currentMode = "AppStore";
        public string CurrentMode
        {
            get
            {
                return _currentMode;
            }
            set
            {
                _currentMode = value;
            }
        }

        #region CRUD operations
        public virtual string Add<T>(T model) where T : IRedisModel
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                if (model == null) return null;
                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    model.Id = NextId<T>().ToString();
                }
                if (Get<T>(model.Id) != null) return null;

                string modelKey = GetKey<T>(model);

                model.ModuleName = CurrentMode;

                Redis.Set<T>(modelKey, model);

                Redis.AddItemToSortedSet(RedisKeyFactory.ListAllKeys<T>(), modelKey, model.CreateDateTime.Ticks);

                Redis.IncrementValue(RedisKeyFactory.ListAllNumKeys<T>());

                Redis.IncrementValue(RedisKeyFactory.NextKey<T>());

                BuildIndex<T>(model);

                return model.Id;
            }
        }

        /// <summary>
        /// since we need remove all index key for originalApp, we need pass in app instance before update
        /// </summary>
        /// <param name="originalApp">app instance before update</param>
        /// <param name="updatedApp">app instance after update</param>
        public virtual void UpdateWithRebuildIndex<T>(T originalModel, T updatedModel) where T : IRedisModel
        {
            if (originalModel == null) throw new ArgumentNullException("originalModel");
            if (updatedModel == null) throw new ArgumentNullException("updatedModel");

            if (!originalModel.Id.EqualsOrdinalIgnoreCase(updatedModel.Id)) throw new ArgumentException("The two model have different ids.");

            Delete<T>(originalModel, false);
            Add<T>(updatedModel);
        }

        public virtual void Update<T>(T model) where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                string modelKey = GetKey<T>(model);
                Redis.Set<T>(modelKey, model);
            }
        }

        public virtual void Delete<T>(T model, bool IsRemoveSubModel = true) where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                if (model != null)
                {
                    string modelKey = GetKey<T>(model);

                    Redis.Remove(modelKey);

                    Redis.RemoveItemFromSortedSet(RedisKeyFactory.ListAllKeys<T>(), modelKey);

                    Redis.IncrementValueBy(RedisKeyFactory.ListAllNumKeys<T>(), -1);

                    if (GetAllCount<T>() == 0)
                    {
                        Redis.Remove(RedisKeyFactory.ListAllNumKeys<T>());
                    }

                    BuildIndex<T>(model, true);

                    if (IsRemoveSubModel)
                        Redis.Remove(RedisKeyFactory.SubModelKey<T>(model.Id));
                }
            }
        }

        public void SetActive<T>(bool isActive, string id) where T : IRedisModel
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                if (isActive)
                {
                    var model = Get<T>(id);
                    if (model != null)
                    {
                        Redis.AddItemToSortedSet(RedisKeyFactory.ListAllKeys<T>(), GetKey<T>(id), model.CreateDateTime.Ticks);
                    }
                }
                else
                {
                    Redis.RemoveItemFromSortedSet(RedisKeyFactory.ListAllKeys<T>(), GetKey<T>(id));
                }
            }
        }

        public bool IsActive<T>(string id) where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.SortedSetContainsItem(RedisKeyFactory.ListAllKeys<T>(), GetKey<T>(id));
            }
        }

        public bool IsExist<T>(string id)
            where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.ContainsKey(GetKey<T>(id));
            }
        }


        public T Get<T>(string id) where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.Get<T>(GetKey<T>(id));
            }
        }

        public int GetAllCount<T>()
            where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.Get<int>(RedisKeyFactory.ListAllNumKeys<T>());
            }
        }

        public int GetAllCountByMode<T>(List<string> allowedModes)
            where T : IRedisModel
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return GetAllActiveModelIds<T>().IdsToValues<T>().FilterByUIMode<T>(allowedModes).Count;
            }
        }

        public int NextId<T>()
             where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                int id = Redis.Get<int>(RedisKeyFactory.NextKey<T>()) + 1;

                while (IsExist<T>(id.ToString()))
                {
                    id++;
                    Redis.IncrementValue(RedisKeyFactory.NextKey<T>());
                }

                return id;
            }
        }

        public List<string> GetAllActiveModelIds<T>() where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetRangeFromSortedSetDesc(RedisKeyFactory.ListAllKeys<T>(), 0, -1);
            }
        }

        public List<string> GetPagedModelIds<T>(int pageNum, int pageSize, string propertyName = "", bool isAsc = false) where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                int start = (pageNum - 1) * pageSize;
                int end = pageNum * pageSize - 1;

                if (pageNum == 0) // get all
                {
                    start = 0;
                    end = -1;
                }

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    if (isAsc)
                    {
                        return Redis.GetRangeFromSortedSet(RedisKeyFactory.ListAllKeys<T>(), start, end);
                    }
                    else
                    {
                        return Redis.GetRangeFromSortedSetDesc(RedisKeyFactory.ListAllKeys<T>(), start, end);
                    }
                }
                else
                {
                    string queryKey = RedisKeyFactory.QueryKeyWithProperty<T>(propertyName);
                    if (isAsc)
                    {
                        return Redis.GetRangeFromSortedSet(queryKey, start, end);
                    }
                    else
                    {
                        return Redis.GetRangeFromSortedSetDesc(queryKey, start, end);
                    }
                }
            }
        }

        public List<T> GetValuesByIds<T>(List<string> ids, bool needKeyFormat = false)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                List<T> results = new List<T>();
                if (needKeyFormat)
                {
                    for (int i = 0; i < ids.Count; i++)
                    {
                        ids[i] = RedisKeyFactory.ModelKey<T>(ids[i]);
                    }
                    results = Redis.GetValues<T>(ids);
                }
                else
                {
                    results = Redis.GetValues<T>(ids);
                }

                return results == null ? new List<T>() : results;
            }
        }

        public List<string> GetSortedIdsByProperty<T>(List<string> ids, string propertyName, int startNum = 0, int num = 0, bool isDesc = true)
            where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                string tempKey = string.Format("Temp:{0}", Guid.NewGuid().ToString());

                Redis.AddRangeToSortedSet(tempKey, ids, 0.0);

                string intoSetKey = string.Format("OUT:{0}", Guid.NewGuid().ToString());
                Redis.StoreIntersectFromSortedSets(intoSetKey, tempKey, RedisKeyFactory.QueryKeyWithProperty<T>(propertyName));
                Redis.Remove(tempKey);

                if (startNum > 0)
                {
                    startNum--;
                }
                else
                {
                    startNum = 0;
                }

                int endNum = -1;
                if (num == 0) // get all
                {
                    startNum = 0;
                }
                else
                {
                    endNum = startNum + num - 1;
                }
                var sortedIds = isDesc ? Redis.GetRangeFromSortedSetDesc(intoSetKey, startNum, endNum) : Redis.GetRangeFromSortedSet(intoSetKey, startNum, endNum);
                Redis.Remove(intoSetKey);

                return sortedIds;
            }
        }

        public List<string> GetFilteredIdsByPropertyFromSets<T>(List<string> ids, string propertyName)
            where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                string tempSetKey = string.Format("TempSet:{0}", Guid.NewGuid().ToString());

                Redis.AddRangeToSet(tempSetKey, ids);

                var filteredIds = Redis.GetDifferencesFromSet(tempSetKey, RedisKeyFactory.QueryKeyWithProperty<T>(propertyName)).ToList();
                Redis.Remove(tempSetKey);

                return filteredIds;
            }
        }

        public List<string> GetIntersectIdsByPropertyFromSets<T>(List<string> ids, params string[] propertyName)
            where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                string tempSetKey = string.Format("TempSet:{0}", Guid.NewGuid().ToString());

                Redis.AddRangeToSet(tempSetKey, ids);
                List<string> keys = new List<string>();
                foreach (var item in propertyName)
                {
                    keys.Add(RedisKeyFactory.QueryKeyWithProperty<T>(item));
                }
                keys.Add(tempSetKey);
                var filteredIds = Redis.GetIntersectFromSets(keys.ToArray()).ToList();
                Redis.Remove(tempSetKey);

                return filteredIds;
            }
        }

        public List<string> GetIntersectIdsByPropertyFromSets(List<string> ids1, List<string> ids2)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                string tempSetKey1 = string.Format("TempSet:{0}", Guid.NewGuid().ToString());
                string tempSetKey2 = string.Format("TempSet:{0}", Guid.NewGuid().ToString());

                Redis.AddRangeToSet(tempSetKey1, ids1);
                Redis.AddRangeToSet(tempSetKey2, ids2);

                var filteredIds = Redis.GetIntersectFromSets(tempSetKey1, tempSetKey2).ToList();

                Redis.Remove(tempSetKey1);
                Redis.Remove(tempSetKey2);

                return filteredIds;
            }
        }


        public bool Remove(string key)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                return Redis.Remove(key);
            }
        }
        #endregion

        #region SUB Model (one to many relation)
        public int GetSubModelCount<TModel>(string modelId)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetHashCount(RedisKeyFactory.SubModelKey<TModel>(modelId));
            }
        }

        public virtual bool SetSubModel<TModel, TSubModel>(string modelId, TSubModel subModel)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                return Redis.SetEntryInHash(RedisKeyFactory.SubModelKey<TModel>(modelId), GetKey<TSubModel>(subModel), JsonConvert.SerializeObject(subModel));
            }
        }

        public bool SetEntryInHash<T>(string modelId, string subModelId, T subModel)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                return Redis.SetEntryInHash(modelId, subModelId, JsonConvert.SerializeObject(subModel));
            }
        }

        public bool DeleteEntryFromHash(string modelId, string subModelId)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                return Redis.RemoveEntryFromHash(modelId, subModelId);
            }
        }

        public void SetRangeInHash<T>(string modelId, List<KeyValuePair<string, T>> keyValuePairs)
        {
            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();

            if (keyValuePairs == null)
                return;

            foreach (var kv in keyValuePairs)
            {
                keyValues.Add(new KeyValuePair<string, string>(kv.Key, JsonConvert.SerializeObject(kv.Value)));
            }

            using (var Redis = RedisClientManager.GetClient())
            {
                Redis.SetRangeInHash(modelId, keyValues as IEnumerable<KeyValuePair<string, string>>);
            }
        }

        public bool IsExist(string key)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.ContainsKey(key);
            }
        }

        public bool IsExistInHash(string modelId, string subModelId)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.HashContainsEntry(modelId, subModelId);
            }
        }

        public T GetValueFromHash<T>(string modelId, string subModelId)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                var subModelJSONString = Redis.GetValueFromHash(modelId, subModelId);
                if (string.IsNullOrWhiteSpace(subModelJSONString))
                {
                    return default(T);
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>(subModelJSONString);
                }
            }
        }

        public List<T> GetValuesFromHash<T>(string hashId, List<string> keys)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                var values = Redis.GetValuesFromHash(hashId, keys.ToArray());

                if (values != null && values.Count > 0)
                {
                    List<T> returnValues = new List<T>();
                    foreach (var item in values)
                    {
                        returnValues.Add(JsonConvert.DeserializeObject<T>(item));
                    }
                    return returnValues;
                }
                else
                {
                    return default(List<T>);
                }
            }
        }

        public List<T> GetAllValuesFromHash<T>(string modelId)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {

                var all = Redis.GetHashValues(modelId);
                if (all != null && all.Count > 0)
                {
                    List<T> returnValues = new List<T>();
                    foreach (var item in all)
                    {
                        returnValues.Add(JsonConvert.DeserializeObject<T>(item));
                    }
                    return returnValues;
                }
                else
                {
                    return default(List<T>);
                }
            }
        }

        public List<string> GetAllKeysFromHash(string modelId)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetHashKeys(modelId);
            }
        }


        public TSubModel GetSubModel<TModel, TSubModel>(string modelId, string subModelId, bool isFullSubModelKey = false)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                var subModelJSONString = Redis.GetValueFromHash(RedisKeyFactory.SubModelKey<TModel>(modelId), isFullSubModelKey ? subModelId : GetKey<TSubModel>(subModelId));
                if (string.IsNullOrWhiteSpace(subModelJSONString))
                {
                    return default(TSubModel);
                }
                else
                {
                    return JsonConvert.DeserializeObject<TSubModel>(subModelJSONString);
                }
            }
        }

        public List<string> GetAllSubModelIds<TModel>(string modelId)
            where TModel : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetHashKeys(RedisKeyFactory.SubModelKey<TModel>(modelId));
            }
        }

        public List<string> GetAllSubModelIdsByType<TModel, TSubModel>(string modelId)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase
        {
            return GetAllSubModelIds<TModel>(modelId).FilterByType<TSubModel>();
        }

        public List<TSubModel> GetAllSubModelsByType<TModel, TSubModel>(string modelId)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase
        {

            List<TSubModel> subModels = new List<TSubModel>();
            var subModelIds = GetAllSubModelIdsByType<TModel, TSubModel>(modelId).ToArray();
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                List<string> values = Redis.GetValuesFromHash(RedisKeyFactory.SubModelKey<TModel>(modelId), subModelIds);
                foreach (var v in values)
                {
                    if (!string.IsNullOrWhiteSpace(v))
                    {
                        subModels.Add(JsonConvert.DeserializeObject<TSubModel>(v));
                    }
                }

                return subModels;
            }
        }

        public virtual bool DeleteSubModel<TModel, TSubModel>(string modelId, string subModelId)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                return Redis.RemoveEntryFromHash(RedisKeyFactory.SubModelKey<TModel>(modelId), GetKey<TSubModel>(subModelId));
            }
        }

        public bool ExistSubModel<TModel, TSubModel>(string modelId, string subModelId)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.HashContainsEntry(RedisKeyFactory.SubModelKey<TModel>(modelId), GetKey<TSubModel>(subModelId));
            }
        }
        #endregion

        #region Set Operations
        public void AddItemToSet(string setId, string item)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                Redis.AddItemToSet(setId, item);
            }
        }

        public void AddRangeToSet(string setId, List<string> items)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                Redis.AddRangeToSet(setId, items);
            }
        }

        public bool SetContainsItem(string setId, string item)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.SetContainsItem(setId, item);
            }
        }

        public List<string> IntersectSets(params string[] setIds)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetIntersectFromSets(setIds).ToList();
            }
        }

        public List<string> DiffSets(string fromSetId, params string[] setIds)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetDifferencesFromSet(fromSetId, setIds).ToList();
            }
        }

        public List<string> UnionSets(params string[] setIds)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetUnionFromSets(setIds).ToList();
            }
        }

        public List<string> GetAllItemsFromSet(string setId)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetAllItemsFromSet(setId).ToList();
            }
        }
        #endregion

        #region Custom Properties

        public T GetModelWithCustomProperties<T, TCustomProperty>(string modelId)
            where T : IRedisModelBase, IRedisModelWithSubModel
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty
        {
            var model = Get<T>(modelId);
            if (model == null)
            {
                return default(T);
            }
            model.CustomProperties = new Dictionary<string, object>();

            var allCustomProperties = GetAllSubModelIdsByType<T, TCustomProperty>(modelId);
            foreach (var p in allCustomProperties)
            {
                var customProperty = GetSubModel<T, TCustomProperty>(modelId, p, true);
                model.CustomProperties[customProperty.Id] = customProperty.Value;
            }

            return model;
        }

        public TCustomProperty GetCustomPropertyFrom<T, TCustomProperty>(string modelId, string customPropertyId)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty
        {
            return GetSubModel<T, TCustomProperty>(modelId, customPropertyId);
        }

        public void AddCustomPropertyFor<T, TCustomProperty>(string modelId, TCustomProperty customProperty)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty
        {
            SetSubModel<T, TCustomProperty>(modelId, customProperty);

            // BUILD INDEX
            BuildIndexForDynamicElements<T, TCustomProperty>(modelId, customProperty);
        }

        public void UpdateCustomPropertyFor<T, TCustomProperty>(string modelId, TCustomProperty originalTCustomProperty, TCustomProperty updatedTCustomProperty)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty
        {
            DeleteCustomPropertyFor<T, TCustomProperty>(modelId, originalTCustomProperty);
            AddCustomPropertyFor<T, TCustomProperty>(modelId, updatedTCustomProperty);
        }

        public void DeleteCustomPropertyFor<T, TCustomProperty>(string modelId, TCustomProperty customProperty)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty
        {
            if (customProperty == null) return;

            DeleteSubModel<T, TCustomProperty>(modelId, customProperty.Id);

            // DELETE INDEX
            BuildIndexForDynamicElements<T, TCustomProperty>(modelId, customProperty, true);
        }

        public void DeleteWithCustomProperties<T, TCustomProperty>(string modelId)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty
        {
            var model = Get<T>(modelId);

            if (model != null)
            {
                var allCustomProperties = GetAllSubModelsByType<T, TCustomProperty>(modelId);
                foreach (var p in allCustomProperties)
                {
                    DeleteCustomPropertyFor<T, TCustomProperty>(modelId, p);
                }
                Delete<T>(model);
            }
        }
        #endregion

        #region Query Interface

        public List<string> FindIdsByConditions<T>(Dictionary<string, string> conditions)
            where T : IRedisModelBase
        {

            List<string> conditionSets = new List<string>();
            foreach (var key in conditions.Keys)
            {
                conditionSets.Add(RedisKeyFactory.QueryKeyWithPropertyAndValue<T>(key, conditions[key]));
            }
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetIntersectFromSets(conditionSets.ToArray()).ToList();
            }
        }

        public List<string> FindIdsByConditions<T>(List<KeyValuePair<string, string>> conditions)
            where T : IRedisModelBase
        {
            List<string> conditionSets = new List<string>();
            foreach (var c in conditions)
            {
                conditionSets.Add(RedisKeyFactory.QueryKeyWithPropertyAndValue<T>(c.Key, c.Value));
            }
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetIntersectFromSets(conditionSets.ToArray()).ToList();
            }
        }

        public List<string> FindIdsByValueRange<T>(string propertyName, double? start, double? end)
            where T : IRedisModelBase
        {

            if (!start.HasValue)
            {
                start = double.MinValue;
            }

            if (!end.HasValue)
            {
                end = double.MaxValue;
            }
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetRangeFromSortedSetByLowestScore(RedisKeyFactory.QueryKeyWithProperty<T>(propertyName), start.GetValueOrDefault(), end.GetValueOrDefault());
            }

        }

        public List<string> FindIdsByValueRange<T>(string propertyName, DateTime? start, DateTime? end)
            where T : IRedisModelBase
        {

            if (!start.HasValue)
            {
                start = DateTime.MinValue;
            }

            if (!end.HasValue)
            {
                end = DateTime.MaxValue;
            }
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetRangeFromSortedSetByLowestScore(RedisKeyFactory.QueryKeyWithProperty<T>(propertyName), start.GetValueOrDefault().Ticks, end.GetValueOrDefault().Ticks);
            }
        }

        public List<string> FuzzyFindIdsByCondition<T>(string property, string valuePattern)
            where T : IRedisModelBase
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetUnionFromSets(KeyFuzzyFind(RedisKeyFactory.QueryKeyWithPropertyAndValue<T>(property, valuePattern)).ToArray()).ToList();
            }
        }

        public List<string> KeyFuzzyFind(string generalKeyPattern)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.SearchKeys(generalKeyPattern);
            }
        }
        #endregion

        #region Index
        public void DoIndexBySet<T>(string idVal, string propertyName, string value, bool isRemoveIndex = false)
            where T : IRedisModelBase
        {
            string queryKey = RedisKeyFactory.QueryKeyWithPropertyAndValue<T>(propertyName, value);
            using (var Redis = RedisClientManager.GetClient())
            {
                if (isRemoveIndex)
                {
                    Redis.RemoveItemFromSet(queryKey, idVal);
                }
                else
                {
                    Redis.AddItemToSet(queryKey, idVal);
                }
            }
        }

        public void DoIndexBySortedSet<T>(string idVal, string propertyName, object value, bool isRemoveIndex = false)
            where T : IRedisModelBase
        {
            string queryKey = RedisKeyFactory.QueryKeyWithProperty<T>(propertyName);
            using (var Redis = RedisClientManager.GetClient())
            {
                if (isRemoveIndex)
                {
                    Redis.RemoveItemFromSortedSet(queryKey, idVal);
                }
                else
                {
                    if (value.GetType().Equals(typeof(DateTime)))
                    {
                        Redis.AddItemToSortedSet(queryKey, idVal, ((DateTime)value).Ticks);
                    }
                    else
                    {
                        Redis.AddItemToSortedSet(queryKey, idVal, Convert.ToDouble(value));
                    }
                }
            }
        }

        public void BuildIndex<T>(T model, bool isRemoveIndex = false)
            where T : IRedisModelBase
        {
            string entityIdValue = GetKey<T>(model);

            foreach (PropertyInfo prop in model.GetType().GetProperties())
            {
                var value = prop.GetValue(model, null);
                if (value == null) continue;

                foreach (object attribute in prop.GetCustomAttributes(true))
                {
                    if (attribute is QueryOrSortFieldAttribute)
                    {
                        if (prop.PropertyType.IsEnumerableType())
                        {
                            var list = value as IEnumerable;
                            if (list == null) continue;

                            foreach (var v in list)
                            {
                                DoIndexBySet<T>(entityIdValue, prop.Name, v.ToString(), isRemoveIndex);
                            }
                        }
                        else if (value.GetType().IsValueType && !value.GetType().Equals(typeof(bool)))
                        {
                            DoIndexBySortedSet<T>(entityIdValue, prop.Name, value, isRemoveIndex);
                        }
                        else
                        {
                            DoIndexBySet<T>(entityIdValue, prop.Name, value.ToString(), isRemoveIndex);
                        }

                        break;
                    }
                }

            }
        }
        #endregion

        #region Helpers
        private bool TryPing(string strIpAddressWithPort, int nTimeoutMsec)
        {
            var tempArray = strIpAddressWithPort.Split(':');
            if (tempArray == null || tempArray.Length < 2)
            {
                return false;
            }

            var strIpAddress = tempArray[0];
            var intPort = tempArray[1].ToInt32();
            Socket connSocket = null;
            try
            {
                connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);


                IAsyncResult result = connSocket.BeginConnect(strIpAddress, intPort, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(nTimeoutMsec, true);

                return connSocket.Connected;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (null != connSocket)
                    connSocket.Close();
            }
        }

        private void BuildIndexForDynamicElements<T, TCustomProperty>(string modelId, TCustomProperty customProperty, bool isRemoveIndex = false)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty
        {
            if (!customProperty.IsQueriable)
            {
                return;
            }

            if (customProperty.Value != null)
            {
                if (customProperty.Value.GetType().IsEnumerableType())
                {
                    foreach (var v in customProperty.Value as IEnumerable)
                    {
                        DoIndexBySet<T>(RedisKeyFactory.ModelKey<T>(modelId), customProperty.Id, v.ToString(), isRemoveIndex);
                    }
                }
                else if (customProperty.Value.GetType().IsValueType && !customProperty.Value.GetType().Equals(typeof(bool)))
                {
                    DoIndexBySortedSet<T>(RedisKeyFactory.ModelKey<T>(modelId), customProperty.Id, customProperty.Value, isRemoveIndex);
                }
                else
                {
                    DoIndexBySet<T>(RedisKeyFactory.ModelKey<T>(modelId), customProperty.Id, customProperty.Value.ToString(), isRemoveIndex);
                }
            }

        }

        protected string GetKey<T>(T model) where T : IRedisModelBase
        {
            return RedisKeyFactory.ModelKey<T>(model.Id);
        }

        private string GetKey<T>(string id)
        {
            return RedisKeyFactory.ModelKey<T>(id);
        }


        #endregion

        #region Queue
        public void AddItemToQueue<T>(string queueId, T queueItem)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                Redis.AddItemToList(RedisKeyFactory.QueueKey<T>(queueId), JsonConvert.SerializeObject(queueItem));
            }
        }

        public T RetrieveItemFromQueue<T>(string queueId)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                var result = Redis.BlockingDequeueItemFromList(RedisKeyFactory.QueueKey<T>(queueId), new TimeSpan(0));

                if (result != null)
                {
                    return JsonConvert.DeserializeObject<T>(result);
                }

                return default(T);
            }
        }

        public List<T> GetAllItemsFromQueue<T>(string queueId)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                var result = Redis.GetAllItemsFromList(RedisKeyFactory.QueueKey<T>(queueId));
                List<T> items = new List<T>();

                if (result != null)
                {
                    foreach (var json in result)
                    {
                        items.Add(JsonConvert.DeserializeObject<T>(json));
                    }
                    return items;
                }

                return items;
            }
        }

        public int GetLengthOfQueue<T>(string queueId)
        {
            using (var Redis = RedisClientManager.GetReadOnlyClient())
            {
                return Redis.GetListCount(RedisKeyFactory.QueueKey<T>(queueId));
            }
        }

        public int RemoveItemFromQueue<T>(string queueId, T queueItem)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                return Redis.RemoveItemFromList(RedisKeyFactory.QueueKey<T>(queueId), JsonConvert.SerializeObject(queueItem));
            }
        }
        #endregion

        #region Misc
        public long GetNextSequenceNum(string key)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                return Redis.IncrementValue(key);
            }
        }

        public bool IsAvailable(int connectionTimeoutMillesecs = 200)
        {
            var redisServers = AppConfigKeys.REDIS_READ_WRITE_SERVERS.ConfigValue().Split(';');
            if (redisServers.Length > 0)
            {
                if (TryPing(redisServers[0], connectionTimeoutMillesecs))
                {
                    using (var Redis = RedisClientManager.GetReadOnlyClient())
                    {
                        if (((RedisClient)Redis).Ping())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public int IncrementValueInHash(string hashKey, string key, int incrementBy = 1)
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                return Redis.IncrementValueInHash(hashKey, key, incrementBy);
            }
        }

        public string IPDetail(long ipVal)
        {
            if ((ipVal >= 167772160 &&
                ipVal <= 184549375) ||
                (ipVal >= 2886729728 &&
                ipVal <= 2887778303) ||
                (ipVal >= 3232235520 &&
                ipVal <= 3232301055))
            {
                return "Local area network";
            }

            const string IP_Ranges_key = "IP_Ranges";
            string ret = string.Empty;
            if (ipVal < 0)
            {
                return ret;
            }

            using (var redis = RedisClientManager.GetReadOnlyClient())
            {
                var redisClient = redis as RedisClient;
                var lastIndex = redisClient.ZCard(IP_Ranges_key) - 1;


                string foundKey = string.Empty;
                int start = 0;
                int end = lastIndex;


                while (string.IsNullOrEmpty(foundKey))
                {
                    var middleIndex = start == 0 ?
                        (int)Math.Floor((end - start) / 2.0d + start)
                        : (int)Math.Ceiling((end - start) / 2.0d + start);
                    var middleVals = redisClient.GetRangeWithScoresFromSortedSet(IP_Ranges_key, middleIndex, middleIndex + 1);

                    if (middleVals.Keys.Count == 2)
                    {
                        var middleItem = middleVals.ElementAt(0);
                        var middleItemNext = middleVals.ElementAt(1);
                        if (middleItem.Value <= ipVal && middleItemNext.Value > ipVal)
                        {
                            foundKey = middleItem.Key;
                            break;
                        }
                        else if (middleItemNext.Value == ipVal)
                        {
                            foundKey = middleItemNext.Key;
                            break;
                        }
                        else if (ipVal < middleItem.Value)
                        {
                            end = middleIndex;
                        }
                        else if (ipVal > middleItemNext.Value)
                        {
                            start = middleIndex + 1;
                        }
                    }
                    else if (middleVals.Keys.Count == 1)
                    {
                        var middleItem = middleVals.ElementAt(0);
                        if (middleItem.Value <= ipVal)
                        {
                            foundKey = middleItem.Key;
                            break;
                        }
                    }
                    else
                    {
                        // finish search , no result found
                        break;
                    }
                }


                return redisClient.Get<string>(foundKey);
            }
        }



        #endregion

        public void FlushAll()
        {
            using (var Redis = RedisClientManager.GetClient())
            {
                Redis.FlushAll();
            }
        }

        #region Dispose
        ~RedisService()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (var Redis = RedisClientManager.GetClient())
                {
                    Redis.Dispose();
                }
            }
        }
        #endregion
    }
}
