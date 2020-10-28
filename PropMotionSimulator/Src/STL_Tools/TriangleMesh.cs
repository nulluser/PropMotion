/**
  ******************************************************************************
  * @file    TriangleMesh.cs
  * @author  Ali Batuhan KINDAN
  * @version V1.0.0
  * @date    03.07.2018
  * @brief   
  ******************************************************************************
  */

using OpenTK;

namespace STL_Tools
{
    public class TriangleMesh
    {
        public Vector3 vert1, normal1;
        public Vector3 vert2, normal2;
        public Vector3 vert3, normal3;

        /**
        * @brief  Class instance constructor
        * @param  none
        * @retval none
        */
        public TriangleMesh()
        {
            vert1 = new Vector3(); normal1 = new Vector3();
            vert2 = new Vector3(); normal2 = new Vector3();
            vert3 = new Vector3(); normal3 = new Vector3();
        }
    }

}
