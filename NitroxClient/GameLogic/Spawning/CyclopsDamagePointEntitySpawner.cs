using System.Collections;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class CyclopsDamagePointEntitySpawner : SyncEntitySpawner<CyclopsDamagePointEntity>
{
    protected override IEnumerator SpawnAsync(CyclopsDamagePointEntity entity, TaskResult<Optional<GameObject>> result)
    {
        SpawnSync(entity, result);
        yield break;
    }

    protected override bool SpawnSync(CyclopsDamagePointEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!NitroxEntity.TryGetComponentFrom(entity.ParentId, out SubRoot subRoot))
        {
            return true;
        }
        CyclopsExternalDamageManager cyclopsExternalDamageManager = subRoot.GetComponentInChildren<CyclopsExternalDamageManager>(true);

        CyclopsDamagePoint damagePoint = cyclopsExternalDamageManager.damagePoints[entity.DamagePointIndex];
        GameObject fxPrefabObject = cyclopsExternalDamageManager.fxPrefabs[entity.FXPrefabIndex];

        // Simulating CyclopsExternalDamageManager.CreatePoint but without the random part

        damagePoint.gameObject.SetActive(true);
        damagePoint.liveMixin.health = entity.Health;
        damagePoint.SpawnFx(fxPrefabObject);
        cyclopsExternalDamageManager.unusedDamagePoints.Remove(damagePoint);

        NitroxEntity.SetNewId(damagePoint.gameObject, entity.Id);

        // As in CyclopsExternalDamageManager.OnTakeDamage
        cyclopsExternalDamageManager.ToggleLeakPointsBasedOnDamage();

        return true;
    }

    protected override bool SpawnsOwnChildren(CyclopsDamagePointEntity entity) => true;
}
