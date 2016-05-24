using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TileMap : MonoBehaviour
{
    public GameObject parentCube;
    public GameObject cube;
    public int roomCount;
    int counter = 0;
    struct Room
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;
        public int width;
        public int height;
    }

    enum IntersectVal
    {
        up,
        down,
        left,
        right,
        none
    }

    public int size_x = 100;
    public int size_z = 50;
    public float tileSize = 1.0f;

    // Use this for initialization
    void Start()
    {
        BuildMesh();
    }

    public void BuildMesh()
    {
        counter = 0;
        int numTiles = size_x * size_z;
        int numTris = numTiles * 2;

        int vsize_x = size_x + 1;
        int vsize_z = size_z + 1;
        int numVerts = vsize_x * vsize_z;

        // Generate the mesh data
        Vector3[] vertices = new Vector3[numVerts];
        Vector3[] normals = new Vector3[numVerts];
        Vector2[] uv = new Vector2[numVerts];

        int[] triangles = new int[numTris * 3];

        int x, z;
        for (z = 0; z < vsize_z; z++)
        {
            for (x = 0; x < vsize_x; x++)
            {
                vertices[z * vsize_x + x] = new Vector3(x * tileSize, 0, z * tileSize);
                normals[z * vsize_x + x] = Vector3.up;
                uv[z * vsize_x + x] = new Vector2((float)x / vsize_x, (float)z / vsize_z);
            }
        }
        //Debug.Log("Done Verts!");

        for (z = 0; z < size_z; z++)
        {
            for (x = 0; x < size_x; x++)
            {
                int squareIndex = z * size_x + x;
                int triOffset = squareIndex * 6;
                triangles[triOffset + 0] = z * vsize_x + x + 0;
                triangles[triOffset + 1] = z * vsize_x + x + vsize_x + 0;
                triangles[triOffset + 2] = z * vsize_x + x + vsize_x + 1;

                triangles[triOffset + 3] = z * vsize_x + x + 0;
                triangles[triOffset + 4] = z * vsize_x + x + vsize_x + 1;
                triangles[triOffset + 5] = z * vsize_x + x + 1;
            }
        }

        //Debug.Log("Done Triangles!");

        // Create a new Mesh and populate with the data
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        // Assign our mesh to our filter/renderer/collider
        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        MeshCollider mesh_collider = GetComponent<MeshCollider>();

        mesh_filter.mesh = mesh;
        mesh_collider.sharedMesh = mesh;
        Debug.Log("Done Mesh!");
        GenerateRooms();
    }

    void GenerateRooms()
    {
        //List<Vector2> roomSizes = new List<Vector2>();
        //for(int j = 2; j<7; j++)
        //{
        //    for(int i = 2; i<7; i++)
        //    {
        //        Vector2 thisSize = new Vector2();
        //        thisSize.x = j;
        //        thisSize.y = i;
        //        roomSizes.Add(thisSize);
        //    }
        //}
        List<Room> rooms = new List<Room>();
        for(int i = 0; i< roomCount; i++)
        {
            int randX = Random.Range(0, size_x-3);
            int randY = Random.Range(4, size_z);
            //int thisRSI = Random.Range(0, roomSizes.Count - 1);

            Room tempRoom;
            int w, h;
            if (size_x - randX > 7 )
            {
                w = Random.Range(2, 7);
            } else
            {
               w = Random.Range(2, size_x - randX - 1);
            }
            if (randY > 7)
            {
                h = Random.Range(2, 7);
            } else
            {
                h = Random.Range(2, randY);
            }

            tempRoom.x1 = randX;
            tempRoom.y1 = randY;
            tempRoom.x2 = w + randX + 1;
            tempRoom.y2 = randY- h - 1;
            tempRoom.width = w;
            tempRoom.height = h;

            rooms.Add(tempRoom);

            
        }
        int timeout = 7;
        for(int i = 0; i< timeout; i++)
        {
            if (rooms.Count > 1)
            {
                for(int p = 0; p < rooms.Count-1; p++)
                for (int k = 0; k < rooms.Count - 1; k++)
                {
                    Room room1, room2 = new Room();
                    room1 = rooms[k];
                    room2 = rooms[p];
                    IntersectVal whereIntersect = intersect(room1, room2);
                    if (whereIntersect != IntersectVal.none  && p != k)
                    {
                        if (whereIntersect == IntersectVal.left)
                        {
                            print("moved left: " + room2.x1 + " " + room2.x2);
                            int moveLeft = room2.x2 - room1.x1 + 1;
                            room2.x1 -= moveLeft;
                            room2.x2 -= moveLeft;
                            print("Update: " + room2.x1 + " " + room2.x2);
                        }
                        else if (whereIntersect == IntersectVal.right)
                        {
                            print("moved right: " + room2.x1 + " " + room2.x2);
                            int moveRight = room1.x2 - room2.x1 + 1;
                            room2.x1 += moveRight;
                            room2.x2 += moveRight;
                            print("Update: " + room2.x1 + " " + room2.x2);
                        }
                        else if (whereIntersect == IntersectVal.up)
                        {
                            print("moved up: " + room2.y1 + " " + room2.y2);
                            int moveUp = room1.y1 - room2.y2 + 1;
                            room2.y1 += moveUp;
                            room2.y2 += moveUp;
                            print("Update: " + room2.y1 + " " + room2.y2);
                        }
                        else if (whereIntersect == IntersectVal.down)
                        {
                            print("moved down: " + room2.y1 + " " + room2.y2);
                            int moveDown = room2.y1 - room1.y2 + 1;
                            room2.y1 -= moveDown;
                            room2.y2 -= moveDown;
                            print("Update: " + room2.y1 + " " + room2.y2);
                        }
                        rooms[p] = room2;
                    }
                }
            }
        }



        //for(int j = 0; j < roomCount; j++)
        //{
        //    for(int i = 0; i < roomCount; i++)
        //    {
        //        intersect(rooms[j], rooms[i]);
        //    }
        //}


        for (int i = 0; i<roomCount; i++)
        {

            GenerateCube(rooms[i]);
        }
    }

    IntersectVal intersect(Room room1, Room room2)
    {
        //if(room2.x1 > room1.x1 && room2.x1 < room1.x2 && room2.y1 < room1.y1 && room2.y1 > room1.y2)
        //{
        //    print("intersect");
        //}
        if(room1.x1 >= room2.x1 && room1.x1 <= room2.x2 && room1.y1 >= room2.y2 && room1.y1 <= room2.y1 //1 Center
        || room1.x1 <  room2.x1 && room1.y1 >  room2.y1 && room1.x2 >  room2.x1 && room1.y2 <  room2.y1
        || room1.x1 >= room2.x1 && room1.x1 <= room2.x2 && room1.y2 <= room2.y1 && room1.y1 >  room2.y1  //Above
        || room1.y1 >= room2.y2 && room1.y1 <= room2.y1 && room1.x2 >= room2.x1 && room1.x1 <  room2.x1  //To the left
          )
        {
            //PrintRoom(room1);
            //PrintRoom(room2);
            //print(room1.x1 >= room2.x1 && room1.x1 <= room2.x2 && room1.y1 > room2.y2 && room1.y1 < room2.y1);
            //print((room1.x1 >= room2.x1 && room1.x1 <= room2.x2 && room1.y2 < room2.y1)  + "  "+ room1.x1 + " " + room2.x1);
            //print((room1.y1 >= room2.y2 && room1.y1 <= room2.y1 && room1.x2 > room2.x1) + "  " + room1.y1 + " " + room2.y2);
            int moveAbove;
            int moveBelow;
            int moveLeft;
            int moveRight;

            moveLeft = room2.x2 - room1.x1;
            moveRight = room1.x2 - room2.x1;

            moveAbove = room1.y1 - room2.y2;
            moveBelow = room2.y1 - room1.y2;
            int minVal = moveLeft;
            if (minVal > moveRight)
                minVal = moveRight;
            if (minVal > moveAbove)
                minVal = moveAbove;
            if (minVal > moveBelow)
                minVal = moveBelow;

            print("left: " + moveLeft);
            print(moveRight);
            print(moveAbove);
            print(moveBelow);

            print(minVal);

            if(moveLeft == minVal)
            {
                return IntersectVal.left;
            } else if( moveRight == minVal)
            {
                return IntersectVal.right;
            } else if (moveAbove == minVal)
            {
                return IntersectVal.up;
            } else if (moveBelow == minVal)
            {
                return IntersectVal.down;
            }


            print("actual intersect");
        }
        return IntersectVal.none;
    }

    void GenerateCube(Room room)
    {
        if(parentCube == null)
        {
            parentCube = new GameObject();
            parentCube.name = "Parent Cube";
        }
        //GameObject init = Instantiate(cube, new Vector3(room.x1+0.5f, 0, room.y1+0.5f), Quaternion.identity) as GameObject;
        //init.transform.parent = parentCube.transform;
        GameObject thisCubeInit = new GameObject();
        thisCubeInit.name = "Cube Collection " + counter;
        thisCubeInit.transform.parent = parentCube.transform;
        for (int i = room.x1; i < room.x2+ 1; i++)
        {
            GameObject init = Instantiate(cube, new Vector3(i + 0.5f, 0, room.y1 + 0.5f), Quaternion.identity) as GameObject;
            init.transform.parent = thisCubeInit.transform;
            init.name = "xPos " + counter;
            init = Instantiate(cube, new Vector3(i + 0.5f, 0, room.y2 + 0.5f), Quaternion.identity) as GameObject;
            init.transform.parent = thisCubeInit.transform;
            init.name = "xPos " + counter;
        }
        for (int i = room.y1; i > room.y2; i--)
        {
            GameObject init = Instantiate(cube, new Vector3(room.x1 + 0.5f, 0, i + 0.5f), Quaternion.identity) as GameObject;
            init.transform.parent = thisCubeInit.transform;
            init.name = "Cube " + counter;

            init = Instantiate(cube, new Vector3(room.x2 + 0.5f, 0, i + 0.5f), Quaternion.identity) as GameObject;
            init.transform.parent = thisCubeInit.transform;
            init.name = "Cube " + counter;
        }
        counter++;

    }

    void PrintRoom(Room a)
    {
        print(a.x1 + " " + a.y1 + " " + a.x2 + " " + a.y2);
    }

}
        //do
        //{
        //    for(int j = 0; j<rooms.Count; j++)
        //    {
        //        for(int i = j+1; i<rooms.Count; i++)
        //        {
        //            Room r1 = rooms[j];
        //            Room r2 = rooms[i];

        //            int xCollide = intersect(r1.x1, r1.x2, r2.x1, r2.x2);
        //            int yCollide = intersect(r1.y1, r1.y2, r2.y1, r2.y2);

        //            if (xCollide != 0 && yCollide != 0)
        //            {
        //                continueLoop = true;

        //                if (Mathf.Abs(xCollide) < Mathf.Abs(yCollide))
        //                {
        //                    int shift1 = (int)Mathf.Floor(xCollide * 0.5f);
        //                    int shift2 = -1 * (xCollide - shift1);

        //                    r1.x1 += shift1;
        //                    r1.x2 += shift1;
        //                    r2.x1 += shift2;
        //                    r2.x2 += shift2;
        //                } else
        //                {
        //                    int shift1 = (int)Mathf.Floor(yCollide * 0.5f);
        //                    int shift2 = -1 * (yCollide - shift1);

        //                    r1.y1 += shift1;
        //                    r1.y2 += shift1;
        //                    r2.y1 += shift2;
        //                    r2.y2 += shift2;
        //                }
        //            }
        //        }
        //    }

        //    loopTimeout--;
        //    if(loopTimeout <= 0)
        //    {
        //        continueLoop = false;
        //        print("timeout");
        //    }

        //} while (continueLoop);