# CompostYield

Plugin BepInEx pour Valheim. Le compost d'[OdinArchitect](https://thunderstore.io/c/valheim/p/OdinPlus/OdinArchitect/)
transforme la viande et le poisson en appats de peche, mais toujours un appat par piece de viande.
Ce plugin multiplie ce rendement — par defaut **1 viande -> 5 appats**.

Le plugin s'installe sur les clients : dans Valheim, une station de ce type n'est simulee que par le
joueur qui se trouve a cote, jamais par le serveur. Il n'est pas obligatoire — un joueur qui ne l'a
pas garde simplement le rendement d'origine.

## Configuration

Fichier `BepInEx/config/ezuku.mods.compostyield.cfg`, section `1 - General` :

| Reglage | Defaut | Description |
| --- | --- | --- |
| `Lock Configuration` | `true` | La configuration est imposee par le serveur ; seuls les admins peuvent la changer. |
| `Yield multiplier` | `5` | Nombre d'objets produits par conversion, au lieu d'un (1 a 20). |
| `Stations` | `rae_compost` | Noms de prefabs concernes, separes par des virgules. Ajouter `rae_fish_trap` pour le piege a poissons. |

Les valeurs sont synchronisees par le serveur : sa valeur ecrase celle des clients a la connexion, et
un changement est applique a chaud, sans redemarrage.

## Licence

MIT, voir [LICENSE](LICENSE). Inclut [ServerSync](https://github.com/blaxxun-boop/ServerSync) de blaxxun.
