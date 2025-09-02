/*
 * Singularity
 * Copyright © 2025 Byteblazar <byteblazar@protonmail.com> * 
 * 
 * 
 * This file is part of Singularity.
 * 
 * Singularity is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * Singularity is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with Singularity. If not, see <https://www.gnu.org/licenses/>. 
 * 
*/


/* Vanilla Classes for AITask/AITarget:
 * EntityAnimalRabbit (also used by chicken)
 * EntityAnimalSnake
 * EntityAnimalStag
 * EntityBackpack
 * EntityBandit
 * EntityEnemyAnimal
 * EntityLootContainer
 * EntityMinibike
 * EntityNPC
 * EntityPlayer
 * EntitySupplyCrate
 * EntitySupplyPlane
 * EntitySurvivor
 * EntityVulture
 * EntityZombie
 * EntityZombieCop
 * EntityZombieDog
 */

public class EntityAnimalChicken : EntityAnimalRabbit { }
public class EntityAnimalSupernatural : EntityEnemyAnimal { }
public class EntityAnimalBear : EntityEnemyAnimal { }
public class EntityAnimalMountainLion : EntityEnemyAnimal { }
public class EntityAnimalBoar : EntityEnemyAnimal { }
public class EntityAnimalWolf : EntityEnemyAnimal { }
public class EntityAnimalCoyote : EntityAnimalWolf { }
public class EntityAnimalZombieBear : EntityAnimalSupernatural { }
public class EntityAnimalDireWolf : EntityAnimalSupernatural { }
public class EntityAnimalBossGrace : EntityAnimalSupernatural { }
public class EntityZombieSmart : EntityZombie { }
public class EntityZombieScreamer : EntityZombieSmart { }
