# CompostYield

Le compost d'OdinArchitect transforme la viande et le poisson en appats de peche, mais toujours
**un appat par piece de viande**. Ce plugin multiplie ce rendement.

Par defaut : **1 viande -> 5 appats**.

## A installer sur les clients

Dans Valheim, une station de type `Smelter` n'est simulee que par le **proprietaire de l'objet**,
c'est-a-dire le client du joueur qui se trouve a cote (`UpdateSmelter()` sort immediatement si
`!m_nview.IsOwner()`). Un serveur dedie, lui, n'instancie les objets qu'autour de sa position de
reference, qui reste a (0,0,0) faute de joueur local : il ne simule donc jamais un compost pose
ailleurs dans le monde.

Consequence : le plugin doit etre installe sur les clients, pas seulement sur le serveur. Il n'est
pas obligatoire (`ModRequired = false`) : un joueur qui ne l'a pas peut se connecter normalement,
mais c'est le rendement vanilla (1 pour 1) qui s'applique quand c'est lui qui se trouve pres du
compost au moment de la production.

Entre les joueurs qui l'ont, la version reste verifiee : un client avec une version differente de
celle du serveur est refuse, pour eviter des rendements incoherents.

## Configuration

Fichier `BepInEx/config/ezuku.mods.compostyield.cfg`, section `1 - General` :

| Reglage | Defaut | Description |
| --- | --- | --- |
| `Lock Configuration` | `true` | La configuration est imposee par le serveur ; seuls les admins peuvent la changer. |
| `Yield multiplier` | `5` | Nombre d'objets produits par conversion, au lieu d'un (1 a 20). |
| `Stations` | `rae_compost` | Noms de prefabs concernes, separes par des virgules. Ajouter `rae_fish_trap` pour le piege a poissons. |

Les valeurs sont synchronisees par ServerSync : la valeur du serveur ecrase celle des clients a la
connexion, et un changement cote serveur est pousse a chaud, sans redemarrage.

## Details techniques

Prefix Harmony sur `Smelter.Spawn(string ore, int stack)`, applique uniquement aux stations listees.
Si la pile multipliee depasse `m_maxStackSize` de l'objet produit, le surplus est emis en piles
supplementaires plutot que perdu.
