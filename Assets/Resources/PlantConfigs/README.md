# Plant Config Assets

Place PlantData ScriptableObject assets in this directory.
Create them via the Unity menu: **Assets → Create → Flowcube → Plant Data**

## Suggested configuration

| Asset name          | Display Name | Required Focus (s) | Consecutive Days | Ecosystem Level |
|---------------------|--------------|--------------------|------------------|-----------------|
| PlantData_Grass.asset  | 荧光草         | 900  (15 min)      | 0                | 1               |
| PlantData_Tree.asset   | 发光树         | 2700 (45 min)      | 0                | 2               |
| PlantData_Relic.asset  | 遗迹石碑       | 2700 (45 min)      | 3                | 3               |

Each asset must also reference a prefab from the `Assets/Prefabs/` folder.
