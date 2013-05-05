using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace BlackMamba.Framework.RedisMapper
{
    public interface IRedisService
    {
        string Add<T>(T model) where T : IRedisModel;
        bool Remove(string key);
        void UpdateWithRebuildIndex<T>(T originalModel, T updatedModel) where T : IRedisModel;
        void Update<T>(T model) where T : IRedisModelBase;
        void Delete<T>(T model, bool isRemoveSubModel = true) where T : IRedisModelBase;
        void SetActive<T>(bool isActive, string id) where T : IRedisModel;
        bool IsActive<T>(string id) where T : IRedisModelBase;
        T Get<T>(string id) where T : IRedisModelBase;
        List<string> GetAllActiveModelIds<T>() where T : IRedisModelBase;
        List<string> GetPagedModelIds<T>(int pageNum, int pageSize, string propertyName = "", bool isAsc = false) where T : IRedisModelBase;
        int GetAllCount<T>() where T : IRedisModelBase;
        int GetAllCountByMode<T>(List<string> allowedModes) where T : IRedisModel;
        bool IsExist<T>(string id) where T : IRedisModelBase;
        int NextId<T>() where T : IRedisModelBase;
        List<T> GetValuesByIds<T>(List<string> ids, bool needKeyFormat = false);
        List<string> GetSortedIdsByProperty<T>(List<string> ids, string propertyName, int startNum = 0, int num = 0, bool isDesc = true) where T : IRedisModelBase;
        List<string> GetFilteredIdsByPropertyFromSets<T>(List<string> ids, string propertyName) where T : IRedisModelBase;
        List<string> GetIntersectIdsByPropertyFromSets<T>(List<string> ids, params string[] propertyName) where T : IRedisModelBase;
        List<string> GetIntersectIdsByPropertyFromSets(List<string> ids1, List<string> ids2);

        int GetSubModelCount<TModel>(string modelId);
        bool SetSubModel<TModel, TSubModel>(string modelId, TSubModel subModel)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase;
        TSubModel GetSubModel<TModel, TSubModel>(string modelId, string subModelId, bool isFullSubModelKey = false)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase;
        bool DeleteSubModel<TModel, TSubModel>(string modelId, string subModelId)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase;
        List<string> GetAllSubModelIds<TModel>(string modelId)
            where TModel : IRedisModelBase;
        bool ExistSubModel<TModel, TSubModel>(string modelId, string subModelId)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase;
        List<TSubModel> GetAllSubModelsByType<TModel, TSubModel>(string modelId)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase;
        List<string> GetAllSubModelIdsByType<TModel, TSubModel>(string modelId)
            where TModel : IRedisModelBase
            where TSubModel : IRedisModelBase;
        bool SetEntryInHash<T>(string modelId, string subModelId, T subModel);
        bool DeleteEntryFromHash(string modelId, string subModelId);
        List<string> GetAllKeysFromHash(string modelId);
        T GetValueFromHash<T>(string modelId, string subModelId);
        List<T> GetAllValuesFromHash<T>(string modelId);
        void SetRangeInHash<T>(string modelId, List<KeyValuePair<string, T>> keyValuePairs);
        bool IsExistInHash(string modelId, string subModelId);
        List<T> GetValuesFromHash<T>(string hashId, List<string> keys);
        bool IsExist(string key);

        T GetModelWithCustomProperties<T, TCustomProperty>(string modelId)
            where T : IRedisModelBase, IRedisModelWithSubModel
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty;
        void DeleteWithCustomProperties<T, TCustomProperty>(string modelId)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty;
        TCustomProperty GetCustomPropertyFrom<T, TCustomProperty>(string modelId, string customPropertyId)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty;
        void AddCustomPropertyFor<T, TCustomProperty>(string modelId, TCustomProperty customProperty)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty;
        void UpdateCustomPropertyFor<T, TCustomProperty>(string modelId, TCustomProperty originalTCustomProperty, TCustomProperty updatedTCustomProperty)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty;
        void DeleteCustomPropertyFor<T, TCustomProperty>(string modelId, TCustomProperty customProperty)
            where T : IRedisModelBase
            where TCustomProperty : IRedisModelBase, IRedisCustomProperty;


        void DoIndexBySet<T>(string idVal, string propertyName, string value, bool isRemoveIndex = false) where T : IRedisModelBase;
        void DoIndexBySortedSet<T>(string idVal, string propertyName, object value, bool isRemoveIndex = false) where T : IRedisModelBase;
        void BuildIndex<T>(T model, bool isRemoveIndex = false) where T : IRedisModelBase;

        List<string> FindIdsByConditions<T>(Dictionary<string, string> conditions) where T : IRedisModelBase;
        List<string> FindIdsByConditions<T>(List<KeyValuePair<string, string>> conditions) where T : IRedisModelBase;
        List<string> FindIdsByValueRange<T>(string propertyName, double? start, double? end) where T : IRedisModelBase;
        List<string> FindIdsByValueRange<T>(string propertyName, DateTime? start, DateTime? end) where T : IRedisModelBase;
        List<string> FuzzyFindIdsByCondition<T>(string property, string valuePattern) where T : IRedisModelBase;
        List<string> KeyFuzzyFind(string generalKeyPattern);

        void AddItemToQueue<T>(string queueId, T queueItem);
        T RetrieveItemFromQueue<T>(string queueId);
        List<T> GetAllItemsFromQueue<T>(string queueId);
        int RemoveItemFromQueue<T>(string queueId, T queueItem);
        int GetLengthOfQueue<T>(string queueId);

        bool IsAvailable(int connectionTimeoutMillesecs = 200);
        long GetNextSequenceNum(string key);
        int IncrementValueInHash(string hashKey, string key, int incrementBy = 1);
        string CurrentMode { get; set; }


        #region Set Operations
        void AddItemToSet(string setId, string item);
        void AddRangeToSet(string setId, List<string> items);
        List<string> IntersectSets(params string[] setIds);
        List<string> DiffSets(string fromSetId, params string[] setIds);
        List<string> UnionSets(params string[] setIds);
        List<string> GetAllItemsFromSet(string setId);
        bool SetContainsItem(string setId, string item);
        #endregion

        #region Misc
        string IPDetail(long ipVal);
        #endregion
    }
}
