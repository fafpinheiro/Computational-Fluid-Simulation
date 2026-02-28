using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPH;

namespace Grid
{
    public class HashGrid : MonoBehaviour
    {
        [Header("Simulation space properties")]
        // public int dimensions = 100;
        [Tooltip("X axis that is represented by red color.")] public int dimensionX = 100;
        [Tooltip("Y axis that is represented by green color.")] public int dimensionY = 100;
        [Tooltip("Z axis that is represented by blue color.")] public int dimensionZ = 100;
        public int maximumParticlesPerCell = 500;
        [SerializeField]
        public int[] _neighbourTracker; // To track witch neighbour we are analizing at the moment

        public int[] _neighbourList; // Stores all neighbours of a particle aligned at 'particleIndex * maximumParticlesPerCell * 8'
        public Dictionary<int, List<int>> _hashGrid = new Dictionary<int, List<int>>();  // Hash of cell to particle indices with current list of particles. 

        public void InitNeighbourHashing(int numberOfParticles,float smoothingRadius/*, Vector3 Spawn*/)
        {
            
            _hashGrid.Clear();  // Only needed when resetting the simulation via testBattery approach.
            _neighbourList = new int[numberOfParticles * maximumParticlesPerCell * 8];   // 8 because we consider 8 cells
            _neighbourTracker = new int[numberOfParticles];
            SpatialHashing.CellSize = smoothingRadius * 2; // Setting cell-size h.
          //  SpatialHashing.Dimensions = dimensions;
            SpatialHashing.DimensionX = dimensionX;
            SpatialHashing.DimensionY = dimensionY;
            SpatialHashing.DimensionZ = dimensionZ;
            //Matrix4x4d m = MathConverter.ToMatrix4x4d(transform.localToWorldMatrix);
            for (int i = 0; i < dimensionX ; i++)
                for (int j = 0; j < dimensionY ; j++)
                    for (int k = 0; k < dimensionZ ; k++)
                    {
                        
                       // Debug.DrawLine(new Vector3(i, j, 0), new Vector3(i, j, dimensionZ), Color.red, 5, false);
                      //  Debug.DrawLine(new Vector3(0, j, k), new Vector3(dimensionX, j, k), Color.red, 5, false);
                      //  Debug.DrawLine(new Vector3(i, 0, k), new Vector3(i, dimensionY, k), Color.red, 5, false);
                        _hashGrid.Add(SpatialHashing.Hash(new Vector3Int(i, j, k)), new List<int>()); // Mapping the hash map by dimensions
                    }
           // GameObject.Find()
           /* for (int i = 0; i < dimensionX * 2; i+=2)
                for (int j = 0; j < dimensionY *2 ; j+=2)
                    for (int k = 0; k < dimensionZ *2 ; k+=2)
                    {
                        Debug.DrawLine(new Vector3(i, j, 0), new Vector3(i, j, dimensionZ *2), Color.red, 5, false);
                         Debug.DrawLine(new Vector3(0, j, k), new Vector3(dimensionX*2, j, k), Color.red, 5, false);
                         Debug.DrawLine(new Vector3(i, 0, k), new Vector3(i, dimensionY*2, k), Color.red, 5, false);
                    }*/

        }  

        //Clear hashGrid
        public void ClearHashGrid(int numberOfParticles)
        {
            foreach (var cell in _hashGrid)
            {
                cell.Value.Clear(); //clear the lists of neighbour particles in cells
            }
            //_neighbourTracker = new int[numberOfParticles];
            //_neighbourList = new int[numberOfParticles * maximumParticlesPerCell * 8];
        }

        //Calculate Hashes for all particles
        public void CalcParticleHashes(SPH.SPHParticle[] particles)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                var hash = SpatialHashing.Hash(SpatialHashing.GetCell(particles[i].position));
                //print("particle: " + i + "pos: " + SpatialHashing.GetCell(particles[i].position));
                if (_hashGrid[hash].Count == maximumParticlesPerCell) continue; // Prevent potential UB in neighbourList if more than maxParticlesPerCell are in a cell.
                _hashGrid[hash].Add(i); // Add particle to specific cell
            }
        }

        // For each particle go through all their 8 neighbouring cells and select the ones that are in range (radius). 
        public void CheckParticleInterations(SPH.SPHParticle[] particles,float smoothingRadiusSq)
        {
            Vector3Int currentCell = new Vector3Int(0,0,0);
            int[] nearCells = new int[8]; // any neighboring particle must exist within the directly adjacent 8 cells
            for (int i = 0; i < particles.Length; i++)
            {
                _neighbourTracker[i] = 0;
                currentCell = SpatialHashing.GetCell(particles[i].position); //considered cell
                nearCells = GetNearbyKeys(currentCell, particles[i].position); // neighboring adjacent cells
                var neighbourPos = i * maximumParticlesPerCell * 8; //Start position of neighbour particles of i
                for (int j = 0; j < nearCells.Length; j++)
                {
                    if (!_hashGrid.ContainsKey(nearCells[j])) continue;

                    var neighbourCell = _hashGrid[nearCells[j]];
                    foreach (var potentialParticle in neighbourCell)
                    {
                       // if (potentialNeighbour == i) continue;

                        if ((particles[potentialParticle].position - particles[i].position).sqrMagnitude < smoothingRadiusSq) // Using squared length instead of magnitude for performance
                        {
                            _neighbourList[neighbourPos + _neighbourTracker[i]++] = potentialParticle; // Add neighbour particle to list
                        }
                    }
                }
            }
        }

        
        private int[] GetNearbyKeys(Vector3Int originIndex, Vector3 position)
        {
            Vector3Int[] nearbyBucketIndices = new Vector3Int[8];
            for (int i = 0; i < 8; i++) //8 cause the eight octantes
            {
                nearbyBucketIndices[i] = originIndex;
            }

            if ((originIndex.x + 0.5f) * SpatialHashing.CellSize <= position.x)
            {
                nearbyBucketIndices[4].x += 1;
                nearbyBucketIndices[5].x += 1;
                nearbyBucketIndices[6].x += 1;
                nearbyBucketIndices[7].x += 1;
            }
            else
            {
                nearbyBucketIndices[4].x -= 1;
                nearbyBucketIndices[5].x -= 1;
                nearbyBucketIndices[6].x -= 1;
                nearbyBucketIndices[7].x -= 1;
            }

            if ((originIndex.y + 0.5f) * SpatialHashing.CellSize <= position.y)
            {
                nearbyBucketIndices[2].y += 1;
                nearbyBucketIndices[3].y += 1;
                nearbyBucketIndices[6].y += 1;
                nearbyBucketIndices[7].y += 1;
            }
            else
            {
                nearbyBucketIndices[2].y -= 1;
                nearbyBucketIndices[3].y -= 1;
                nearbyBucketIndices[6].y -= 1;
                nearbyBucketIndices[7].y -= 1;
            }

            if ((originIndex.z + 0.5f) * SpatialHashing.CellSize <= position.z)
            {
                nearbyBucketIndices[1].z += 1;
                nearbyBucketIndices[3].z += 1;
                nearbyBucketIndices[5].z += 1;
                nearbyBucketIndices[7].z += 1;
            }
            else
            {
                nearbyBucketIndices[1].z -= 1;
                nearbyBucketIndices[3].z -= 1;
                nearbyBucketIndices[5].z -= 1;
                nearbyBucketIndices[7].z -= 1;
            }

            int[] nearbyKeys = new int[8];
            for (int i = 0; i < 8; i++)
            {
                nearbyKeys[i] = SpatialHashing.Hash(nearbyBucketIndices[i]);
            }

            return nearbyKeys;
        }

        
    }
}
