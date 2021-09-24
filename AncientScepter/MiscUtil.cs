using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AncientScepter
{
    /// <summary>
    /// Contains miscellaneous utilities for working with a myriad of RoR2/R2API features, as well as some other math/reflection standalones.
    /// </summary>
    public static class MiscUtil
    {
        /// <summary>
        /// A collection of unique class instances which all inherit the same base type.
        /// </summary>
        /// <typeparam name="T">The type to enforce inheritance from for the contents of the FilingDictionary.</typeparam>
        public class FilingDictionary<T> : IEnumerable<T>
        {
            private readonly Dictionary<Type, T> _dict = new Dictionary<Type, T>();

            /// <summary>
            /// Gets the number of instances contained in the FilingDictionary.
            /// </summary>
            public int Count => _dict.Count;

            /// <summary>
            /// Add an object inheriting from <typeparamref name="T"/> to the FilingDictionary. Throws an ArgumentException if an element of the object's type is already contained.
            /// </summary>
            /// <param name="inst">The object to add.</param>
            public void Add(T inst)
            {
                _dict.Add(inst.GetType(), inst);
            }

            /// <summary>
            /// Add an object of type <typeparamref name="subT"/> to the FilingDictionary. Throws an ArgumentException if an element of the object's type is already contained.
            /// </summary>
            /// <typeparam name="subT">The type of the object to add.</typeparam>
            /// <param name="inst">The object to add.</param>
            public void Add<subT>(subT inst) where subT : T
            {
                _dict.Add(typeof(subT), inst);
            }

            /// <summary>
            /// Add an object of type <typeparamref name="subT"/> to the FilingDictionary, or replace one of the same type if it already exists.
            /// </summary>
            /// <typeparam name="subT">The type of the object to insert.</typeparam>
            /// <param name="inst">The object to insert.</param>
            public void Set<subT>(subT inst) where subT : T
            {
                _dict[typeof(subT)] = inst;
            }

            /// <summary>
            /// Attempts to get an object of type <typeparamref name="subT"/> from the FilingDictionary.
            /// </summary>
            /// <typeparam name="subT">The type of the object to retrieve.</typeparam>
            /// <returns>The unique object matching type <typeparamref name="subT"/> within this FilingDictionary if such an object exists; null otherwise.</returns>
            public subT Get<subT>() where subT : T
            {
                return (subT)_dict[typeof(subT)];
            }

            /// <summary>
            /// Removes the given object from the FilingDictionary.
            /// </summary>
            /// <param name="T">The object to remove.</typeparam>
            public void Remove(T inst)
            {
                _dict.Remove(inst.GetType());
            }

            /// <summary>
            /// Removes all objects from the FilingDictionary which match a predicate.
            /// </summary>
            /// <param name="predicate">The predicate to filter removals by.</param>
            public void RemoveWhere(Func<T, bool> predicate)
            {
                foreach (var key in _dict.Values.Where(predicate).ToList())
                {
                    _dict.Remove(key.GetType());
                }
            }

            /// <summary>
            /// Returns an enumerator that iterates through the FilingDictionary's contained objects.
            /// </summary>
            /// <returns>An enumerator that iterates through the FilingDictionary's contained objects.</returns>
            public IEnumerator<T> GetEnumerator()
            {
                return _dict.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Returns a new ReadOnlyFilingDictionary wrapping this FilingDictionary.
            /// </summary>
            /// <returns>A new ReadOnlyFilingDictionary wrapping this FilingDictionary.</returns>
            public ReadOnlyFilingDictionary<T> AsReadOnly() => new ReadOnlyFilingDictionary<T>(this);
        }

        /// <summary>
        /// A readonly wrapper for an instance of FilingDictionary.
        /// </summary>
        /// <typeparam name="T">The type to enforce inheritance from for the contents of the FilingDictionary.</typeparam>
        public class ReadOnlyFilingDictionary<T> : IReadOnlyCollection<T>
        {
            private readonly FilingDictionary<T> baseCollection;

            /// <summary>
            /// Creates a new ReadOnlyFilingDictionary wrapping a specific FilingDictionary.
            /// </summary>
            /// <param name="baseCollection">The FilingDictionary to create a readonly wrapper for.</param>
            public ReadOnlyFilingDictionary(FilingDictionary<T> baseCollection)
            {
                this.baseCollection = baseCollection;
            }

            /// <summary>
            /// Gets the number of instances contained in the wrapped FilingDictionary.
            /// </summary>
            public int Count => baseCollection.Count;

            /// <summary>
            /// Returns an enumerator that iterates through the wrapped FilingDictionary's contained objects.
            /// </summary>
            /// <returns>An enumerator that iterates through the wrapped FilingDictionary's contained objects.</returns>
            public IEnumerator<T> GetEnumerator() => baseCollection.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => baseCollection.GetEnumerator();
        }

        /// <summary>
        /// Returns a list of all CharacterMasters which have living CharacterBodies.
        /// </summary>
        /// <param name="playersOnly">If true, only CharacterMasters which are linked to a PlayerCharacterMasterController will be matched.</param>
        /// <returns>A list of all CharacterMasters which have living CharacterBodies (and PlayerCharacterMasterControllers, if playersOnly is true).</returns>
        public static List<CharacterMaster> AliveList(bool playersOnly = false)
        {
            if (playersOnly) return PlayerCharacterMasterController.instances.Where(x => x.isConnected && x.master && x.master.hasBody && x.master.GetBody().healthComponent.alive).Select(x => x.master).ToList();
            else return CharacterMaster.readOnlyInstancesList.Where(x => x.hasBody && x.GetBody().healthComponent.alive).ToList();
        }

        /// <summary>
        /// Spawn an item of the given tier at the position of the given CharacterBody.
        /// </summary>
        /// <param name="src">The body to spawn an item from.</param>
        /// <param name="tier">The tier of item to spawn. Must be within 0 and 5, inclusive (Tier 1, Tier 2, Tier 3, Lunar, Equipment, Lunar Equipment).</param>
        /// <param name="rng">An instance of Xoroshiro128Plus to use for random item selection.</param>
        public static void SpawnItemFromBody(CharacterBody src, int tier, Xoroshiro128Plus rng)
        {
            List<PickupIndex> spawnList;
            switch (tier)
            {
                case 1:
                    spawnList = Run.instance.availableTier2DropList;
                    break;

                case 2:
                    spawnList = Run.instance.availableTier3DropList;
                    break;

                case 3:
                    spawnList = Run.instance.availableLunarDropList;
                    break;

                case 4:
                    spawnList = Run.instance.availableNormalEquipmentDropList;
                    break;

                case 5:
                    spawnList = Run.instance.availableLunarEquipmentDropList;
                    break;

                case 0:
                    spawnList = Run.instance.availableTier1DropList;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("tier", tier, "spawnItemFromBody: Item tier must be between 0 and 5 inclusive");
            }
            PickupDropletController.CreatePickupDroplet(spawnList[rng.RangeInt(0, spawnList.Count)], src.transform.position, new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), 20f, UnityEngine.Random.Range(-5.0f, 5.0f)));
        }
    }
}