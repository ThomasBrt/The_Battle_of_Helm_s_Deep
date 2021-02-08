Description du jeu :

C'est un jeu de tir dans l'univers du Seigneur des Anneaux.
Le joueur incarne un archer elfe (représenté par le personnage de Legolas) et doit affronter des vagues d'orcs successives,
pour défendre le Gouffre de Helm. 

Gameplay : 

Le joueur peut se déplacer uniquement de gauche à droite, et tirer sur les orcs en face de lui. 
A chaque niveau, une nouvelle vague d'orcs avance vers le joueur en lui lançant des lances.
Si un orc ou une lance touche le joueur, ce dernier perd une vie et la vague est repositionnée à son point de départ, sans affecter le nombre d'orcs restants à ce niveau.
Une vague est constituée de 2 orcs, puis de 4 après le 1er niveau, ensuite seule leur vitesse de marche augmente.
Comme dans les films, Legolas peut tirer trois flêches d'un coup, ce qui n'est possible ici que si le joueur touche l'étoile bonus, obtenable à chaque fin de niveau.

La version proposée s'inspire du fonctionnement de SpaceInvaders (notamment pour la gestion des vagues)

Composition de la scène : 

	- Ecran d'accueil : 
	
		- Titre du jeu (panel WaitToStart)
		- Affichage des meilleurs scores (panel Network)
		- Affichage/changement de pseudo du joeur (panel Network)
		- Description des raccourcis clavier (panel WaitToStart)
		- Bouton Play pour lancer le jeu (panel WaitToStart)

	- Ecran de jeu (visible une fois les panels WaitToStart et Network masqués) :
	
		- Panel GameInfo : LblScore, LblLives, LblLevel
		- Player (visible aussi dans sur l'écran d'accueil, car problème en utilisant la méthode SetActive/ ou destruction du player puis réinstantiation)
			- ArrowPlayer (prefab)
			- gameobject Eject : objet vide qui permet de connaître la position d'instanciation des tirs du player
		- Wave :
			- OrcSoldier (prefab)
		- SpearOrc (prefab) instanciés devant l'orc à la position x où le rayon de détection touche le joueur en face
		- Pause : texte quand le jeu est en pause
		- LblLevelUp : texte quand le joueur termine un niveau
		- Message : texte quand le joueur perd la partie (GameOver)
		- Limits : gameobject vide contenant un Box collider 2d sur chaque côté de l'écran (remplace le script WrapScreen, car problème de mesure de la width)
		- Star : bonus instancié à la fin d'une vague, sur la position du dernier orc tué
		- RockBG : sprite de fond répété, masqué au final car empêche une bonne visibilité

Fonctionnement du jeu :

	- Au lancement du jeu, le joueur a des informations sur les meilleurs scores, peut changer de pseudo ou non, puis lancer une partie en appuyant sur le bouton Play
	- L'interface est modifiée, et le joueur peut voir ses infos de : score, vies et niveaux
	- Peu après une première vague d'orcs de 1 LIGNE apparaît en bas à gauche de l'écran. Pour le 1er niveau, le joueur a droit à un 1er tir bonus.
	- La vague monte vers le joueur, mais pas directement ni en diagonal : 
	comme dans SpaceInvaders, la vague se "cogne" sur un côté, monte d'un cran puis inverse sa direction 
	- Lorsque qu'un orc se trouve en face du joueur (en x), le rayon de détection de présence qui part du joueur autorise l'orc à tirer en respectant une certaine cadence
	- De son côté, le joueur peut se déplacer de gauche à droite et tirer des flêches sur les orcs (à une certaine cadence, qui équivaut au temps avant destruction de la flêche).
	- Quand la fleche touche un orc, celui-ci est détruit et une tâche de sang est instanciée (avec un son) à sa position. La vague continue d'avancer. Le joueur gagne 50 points.
	- Quand tous les orcs sont tués, un étoile bonus est instanciée en même temps et monte vers le joueur. Le bonus lui offre 30 points et lui permettra de tirer 3 fleches parallèles au lieu d'1 au prochain tir.
	- Ensuite le joueur a terminé le niveau : un message apparaît puis une nouvelle vague arrive après un certain délai
	- Le joueur doit penser à esquiver les lances pour ne pas mourir et veiller au rapprochement de la vague, pour ne pas rentrer en contact, ce qui le ferait mourir aussi
	- Si le joueur meurt, il perd une vie et la vague se replace à sa position d'origine, jusqu'à ce qu'il n'ait plus de vies et donc Gameover.
	- Une animation entre 2 sprites permet de signaler la mort du joueur
	- En cas de Gameover, un message avec le score apparaît, puis l'écran d'accueil réapparaît, avec la liste des scores mise à jour.
	

Analyse synthétique des scripts : 

	- GameManager (script de gestion du "cycle de vie" général du jeu) : 
		- Gestion des états de jeu : wait, play, levelup (wait est utilisé quand il y a pause ou GameOver)
		- Gestion des textes de scores, lives et level (les accesseurs remplacent la méthode UpdateTexts() vue en cours pour la modif en temps réel)
		- Gestion de la vitesse et du nombre de lignes par vague (WaveSpeedForLevel, TotalWaveRows)
		- tb_LaunchGame() : masque l'écran d'accueil, initialise les labels de GameInfo avec tb_InitGame(), puis appelle la coroutine tb_waitBeforeNewLevel()
		- tb_waitBeforeNewLevel() : temporise puis lance tb_LoadLevel() => calcul de WaveSpeedForLevel, TotalWaveRows puis appelle tb_LoadWave() dans la classe tb_Wave()
		- tb_GameOverToStart() : lorsque le joueur n'a plus de vie, cette méthode est appelée depuis la classe tb_PlayerController()
		- tb_EndOfLevel() : lorsque que la vague ne contient plus d'orc (int RemainingOrc), cette méthode est appelée depuis la classe tb_Wave()
		
	- tb_Wave() (script de gestion du cycle de vie des vagues d'orcs) :
		- tb_LoadWave() : génère une vague de prefab d'orcs selon des paramètres de position ou de nombre d'orcs par vague/ligne
		- tb_WaveTouchLimit() : renseigne les booleans de déplacement sur le côté ou le vers le haut, s'il y a collision avec le collider du gameObject Limit
		- tb_MoveWave() : génère le déplacement soit sur le côté (et donc de la direction) soit vers le haut
		- tb_StopWave() : la vague ne peut plus avancer (WaveMoving = false)
		- tb_WaveIsEmpty() : stop la vague et active l'état "levelup"; la méthode est appelée par tb_OrcDeath()
		- tb_RestartWave() : repositionne la vague à sa position initiale, et réinitialise le player après sa mort (méthode dans tb_PlayerController())

	- tb_PlayerController() (script de gestion générale du player + détection de tir par les orcs => plus simple car il s'agit d'une collision avec le player) :
		Script organisé en 3 parties : 
		1. Mouvements et tirs du player
		- tb_MovePlayer() : déplacements gauche/droite dans la limite en x de l'écran (on utilise un Clamp)
		- tb_PlayerShoot() : si CanShoot est à true, le joueur peut tirer; pareil pour CanShootBonus (on instancie 2 flêche supplémentaires sur les côtés)

		2. Collision, mort et rénitialisation du player
		- OnTriggerEnter2D() : on détecte la collision entre le player et : un orc, une lance (tb_OrcKillPlayer()) ou une étoile bonus (CanShootBonus à true)
		- tb_OrcKillPlayer() : on stoppe la vague et l'animation de mort du player, puis soit on active le game over, soit le repositionnement de la vague
		- tb_PlayerDeath() : animation de mort du player, il ne peut pas tirer en même temps 
		- tb_InitPlayer() et tb_ReinitPlayer() : dépend s'il s'agit d'une première initialisation ou non, si oui il faut aussi démarrer tb_MovePlayer()
		
		3. Tir d'orc vers le player
		- tb_OrcShoot() : on crée un rayon de détection depuis le joueur vers le bas; si la vague est touchée, alors on autorise un tir d'orc vers la position du player
		- tb_PauseOrcShoot() : on limite la cadence de tir des orcs
		
	- tb_ArrowPlayer() (assigné au prefab ArrowPlayer) : gère la vitesse/direction de la fleche, plus gestion en cas de collision avec un orc (instancie OrcDeath et augmente le score de 50 points)

	- tb_OrcDeath() (assigné au prefab OrcDeath) : 
		- c'est l'objet "tache de sang" qui est instancié après la mort d'un orc, il gère la décrémentation du nombre d'orcs restants dans la vague
		- s'il n'y a plus d'orcs, on appelle tb_IsWaveEmpty() dans la classe tb_Wave(), qui appellera ensuite tb_EndOfLevel() dans le GameManager

	- tb_SpearOrc() et tb_WaveBonus() : ces scripts servent uniquement à faire monter les prefabs correspondants après leur instanciation (SpearOrc et Star); on aurait pu utiliser la gravité du Rigidbody s'il fallait descendre
	
	- tb_NetworkManager() et PauseManager() : les mêmes scripts que vus en cours

	- tb_Limits() (assigné au gameObject Limits) : on détecte la collision d'un orc avec le box collider de gauche ou de droite au bord de l'écran, puis on appelle tb_WaveTouchLimit() pour changer leur direction
							
		
Ressources utilisées : 

Orcs : 
https://opengameart.org/content/orcs

Legolas :
https://www.gifmania.co.uk/Movies-Cinema-Animated-Gifs/Animated-Fantasy-Films/Hobbit-Desolation-Smaug/

Legolas mourant = opacité à 70% avec Photoshop

Squelette :
https://opengameart.org/content/elder-skeleton

Flêche : 
https://opengameart.org/content/arrow-1

Lance :
https://opengameart.org/content/spear-5

Etoile :
https://opengameart.org/content/star-of-dawn

Son de tir de flêche :
https://opengameart.org/content/bow-arrow-shot

Son de mort d'orc (extrait avec Audacity) :
https://opengameart.org/content/grunts-male-death-and-pain

Tâche de sang :
https://opengameart.org/content/blood-splats

Police Aniron (libre hors usage commercial) :
https://www.1001fonts.com/aniron-font.html

Sol rocheux (pas utilisé finalement) :
https://opengameart.org/content/rock-seamless-textures
