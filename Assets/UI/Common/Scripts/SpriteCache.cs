using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace IdleRPG.UI
{
    /// <summary>
    /// Addressable 스프라이트를 캐싱하여 동일 키의 중복 로드를 방지한다.
    /// Presenter 생명주기에 맞춰 Dispose를 호출해야 한다.
    /// </summary>
    public class SpriteCache : IDisposable
    {
        private readonly Dictionary<string, AsyncOperationHandle<Sprite>> _cache = new();
        private bool _disposed;

        /// <summary>
        /// 스프라이트를 캐시에서 조회하거나, 없으면 Addressables로 로드한다.
        /// </summary>
        /// <param name="key">Addressable 스프라이트 키</param>
        /// <param name="token">취소 토큰</param>
        /// <returns>로드된 스프라이트</returns>
        public async UniTask<Sprite> LoadAsync(string key, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key)) return null;

            if (_cache.TryGetValue(key, out var existing) && existing.IsValid())
            {
                if (existing.IsDone) return existing.Result;
                return await existing.Task;
            }

            var handle = Addressables.LoadAssetAsync<Sprite>(key);
            _cache[key] = handle;
            var sprite = await handle.Task;
            token.ThrowIfCancellationRequested();
            return sprite;
        }

        /// <summary>
        /// 캐시된 모든 스프라이트 핸들을 해제한다.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var handle in _cache.Values)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            _cache.Clear();
        }
    }
}
