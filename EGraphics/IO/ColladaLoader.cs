using Erde;
using Erde.Graphics.Variables;
using Erde.IO;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Erde.Graphics.IO
{
    // Dependancy hell with other libraries so doing quick and dirty from scratch
    // Is not the optimal format however being XML makes it easy to work with.
    public class ColladaLoader
    {
        struct MeshParam
        {
            public string Name;
            public string Type;
        }
        struct MeshAccessor
        {
            public string Source;
            public int Stride;
            public uint Count;
            public List<MeshParam> Params;
        }
        struct MeshTechnique
        {
            public MeshAccessor Accessor;
        }
        struct MeshSource
        {
            public string ID;
            public string[] NameData;
            public float[] FloatData;
            public MeshTechnique Technique;
        }

        struct Input
        {
            public string Semantic;
            public string Source;
            public int Offset;
        }

        struct MeshVertex
        {
            public string ID;
            public List<Input> Inputs;
        }

        struct MeshTriangle
        {
            public List<Input> Inputs;
            public uint[] Data;
            public uint Count;
        }

        struct Mesh
        {
            public List<MeshSource> MeshSources;
            public MeshVertex MeshVertex;
            public MeshTriangle MeshTriangle;
        }

        struct Geometry
        {
            public string ID;
            public string Name;
            public Mesh Mesh;
        }

        struct ControllerVertexWeights
        {
            public List<Input> Inputs;
            public uint[] WeightCount;
            public uint[] WeightBonePairs;
            public uint Count;
        }

        struct ControllerJoints
        {
            public List<Input> Inputs;
        }

        struct ControllerSkin
        {
            public string Source;
            public Matrix4 BindShapeMatrix;
            public List<MeshSource> Sources;
            public ControllerJoints Joints; 
            public ControllerVertexWeights Weights;
        }

        struct Controller
        {
            public string ID;
            public string Name;
            public ControllerSkin Skin;
        }

        class SceneNode
        {   
            public string ID;
            public string Name;
            public string Type;
            public SceneNode Parent;
            public List<SceneNode> Children;
            public Matrix4 Transform;
            public string SID;
        }

        struct Scene
        {
            public string ID;
            public string Name;
            public List<SceneNode> Nodes;
        }

        Skeleton                         m_skeleton;

        Dictionary<string, SkeletonNode> m_skeletonNameLookup;

        List<SkeletonNode>               m_skeletonNodes;

        List<Geometry>                   m_geometry;
        List<Controller>                 m_controller;
        List<Scene>                      m_scene;

        public int ModelCount
        {
            get
            {
                return m_geometry.Count;
            }
        }

        MeshAccessor LoadMeshAccessor(XmlNode a_parentNode)
        {
            MeshAccessor meshAccessor = new MeshAccessor();
            meshAccessor.Params = new List<MeshParam>();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "param":
                    {
                        MeshParam param = new MeshParam();

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "name":
                                {
                                    param.Name = att.Value;

                                    break;
                                }
                                case "type":
                                {
                                    param.Type = att.Value;

                                    break;
                                }
                            }
                        }

                        meshAccessor.Params.Add(param);

                        break;
                    }
                }
            }

            return meshAccessor;
        }

        MeshTechnique LoadMeshTechnique(XmlNode a_parentNode)
        {
            MeshTechnique meshTechnique = new MeshTechnique();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "accessor":
                    {
                        MeshAccessor accessor = LoadMeshAccessor(node);

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "source":
                                {
                                    accessor.Source = att.Value;

                                    break;
                                }
                                case "stride":
                                {
                                    accessor.Stride = int.Parse(att.Value);

                                    break;
                                }
                                case "count":
                                {
                                    accessor.Count = uint.Parse(att.Value);

                                    break;
                                }
                            }
                        }

                        meshTechnique.Accessor = accessor;

                        break;
                    }
                }
            }

            return meshTechnique;
        }

        MeshSource LoadMeshSource(XmlNode a_parentNode)
        {
            MeshSource meshSource = new MeshSource();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "float_array":
                    {   
                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "id":
                                {
                                    meshSource.ID = att.Value;

                                    break;
                                }
                            }
                        }

                        string str = node.InnerText;
                        string[] lines = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        int count = lines.Length;

                        meshSource.FloatData = new float[count];

                        for (int i = 0; i < count; ++i)
                        {
                            meshSource.FloatData[i] = float.Parse(lines[i]);
                        }

                        break;
                    }
                    case "Name_array":
                    {
                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "id":
                                {
                                    meshSource.ID = att.Value;

                                    break;
                                }
                            }
                        }

                        string str = node.InnerText;
                        meshSource.NameData = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        break;
                    }
                    case "technique_common":
                    {
                        MeshTechnique meshTechnique = LoadMeshTechnique(node);

                        meshSource.Technique = meshTechnique;

                        break;
                    }
                }
            }

            return meshSource;
        }

        MeshVertex LoadMeshVertex(XmlNode a_parentNode)
        {
            MeshVertex vertex = new MeshVertex();
            vertex.Inputs = new List<Input>();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "input":
                    {
                        Input input = new Input();
                        input.Offset = 0;

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "semantic":
                                {
                                    input.Semantic = att.Value;

                                    break;
                                }
                                case "source":
                                {
                                    input.Source = att.Value;

                                    break;
                                }
                            }
                        }

                        vertex.Inputs.Add(input);

                        break;
                    }
                }
            }

            return vertex;
        }

        MeshTriangle LoadMeshTriangles(XmlNode a_parentNode)
        {
            MeshTriangle triangles = new MeshTriangle();
            triangles.Inputs = new List<Input>();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "input":
                    {
                        Input input = new Input();

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "semantic":
                                {
                                    input.Semantic = att.Value;

                                    break;
                                }
                                case "source":
                                {
                                    input.Source = att.Value;

                                    break;
                                }
                                case "offset":
                                {
                                    input.Offset = int.Parse(att.Value);

                                    break;
                                }
                            }
                        }

                        triangles.Inputs.Add(input);

                        break;
                    }
                    case "p":
                    {
                        string str = node.InnerText;

                        string[] lines = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        int len = lines.Length;
                        triangles.Data = new uint[len];

                        for (int i = 0; i < len; ++i)
                        {
                            triangles.Data[i] = uint.Parse(lines[i]);
                        }

                        break;
                    }
                }
            }

            return triangles;
        }

        Mesh LoadMesh(XmlNode a_parentNode)
        {
            Mesh mesh = new Mesh();
            mesh.MeshSources = new List<MeshSource>();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "source":
                    {
                        MeshSource source = LoadMeshSource(node);

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "id":
                                {
                                    source.ID = att.Value;

                                    break;
                                }
                            }
                        }

                        mesh.MeshSources.Add(source);

                        break;
                    }
                    case "vertices":
                    {
                        MeshVertex vertex = LoadMeshVertex(node);

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "id":
                                {
                                    vertex.ID = att.Value;

                                    break;
                                }
                            }
                        }

                        mesh.MeshVertex = vertex;

                        break;
                    }
                    case "triangles":
                    {
                        MeshTriangle triangles = LoadMeshTriangles(node);

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "count":
                                {
                                    triangles.Count = uint.Parse(att.Value);

                                    break;
                                }
                            }
                        }

                        mesh.MeshTriangle = triangles;

                        break;
                    }
                }
            }

            return mesh;
        }

        Geometry LoadGeometry(XmlNode a_parentNode)
        {
            Geometry geometry = new Geometry();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "mesh":
                    {
                        geometry.Mesh = LoadMesh(node);   

                        break;
                    }
                }
            }

            return geometry;
        }

        void LoadGeometryLibrary(XmlNode a_parentNode)
        {
            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "geometry":
                    {
                        Geometry geometry = LoadGeometry(node);

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "id":
                                {
                                    geometry.ID = att.Value;

                                    break;
                                }
                                case "name":
                                {
                                    geometry.Name = att.Value;

                                    break;
                                }
                            }
                        }

                        m_geometry.Add(geometry);

                        break;
                    }
                }
            }
        }

        ControllerJoints LoadControllerJoints(XmlNode a_parentNode)
        {
            ControllerJoints joints = new ControllerJoints();
            joints.Inputs = new List<Input>();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "input":
                    {
                        Input input = new Input();
                        input.Offset = 0;

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "semantic":
                                {
                                    input.Semantic = att.Value;

                                    break;
                                }
                                case "source":
                                {
                                    input.Source = att.Value;

                                    break;
                                }
                            }
                        }

                        joints.Inputs.Add(input);

                        break;
                    }
                }
            }

            return joints;
        }

        ControllerVertexWeights LoadControllerVertexWeights(XmlNode a_parentNode)
        {
            ControllerVertexWeights weights = new ControllerVertexWeights();
            weights.Inputs = new List<Input>();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "input":
                    {
                        Input input = new Input();

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "semantic":
                                {
                                    input.Semantic = att.Value;

                                    break;
                                }
                                case "source":
                                {
                                    input.Source = att.Value;

                                    break;
                                }
                                case "offset":
                                {
                                    input.Offset = int.Parse(att.Value);

                                    break;
                                }
                            }
                        }

                        weights.Inputs.Add(input);

                        break;
                    }
                    case "vcount":
                    {
                        string str = node.InnerText;

                        string[] lines = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int len = lines.Length;

                        weights.WeightCount = new uint[len];
                        for (int i = 0; i < len; ++i)
                        {
                            weights.WeightCount[i] = uint.Parse(lines[i]);
                        }

                        break;
                    }
                    case "v":
                    {
                        string str = node.InnerText;

                        string[] lines = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);   
                        int len = lines.Length;

                        weights.WeightBonePairs = new uint[len];
                        for (int i = 0; i < len; ++i)
                        {
                            weights.WeightBonePairs[i] = uint.Parse(lines[i]);
                        }

                        break;
                    }
                }
            }

            return weights;
        }

        ControllerSkin LoadControllerSkin(XmlNode a_parentNode)
        {
            ControllerSkin skin = new ControllerSkin();
            skin.Sources = new List<MeshSource>();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "bind_shape_matrix":
                    {
                        Matrix4 mat = Matrix4.Identity;

                        string str = node.InnerText;
                        string[] lines = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // TODO: Double check matrix orientation 
                        for (int x = 0; x < 4; ++x)
                        {
                            for (int y = 0; y < 4; ++y)
                            {
                                mat[x, y] = float.Parse(lines[x + (y * 4)]);
                            }
                        }

                        skin.BindShapeMatrix = mat;

                        break;
                    }
                    case "source":
                    {
                        MeshSource source = LoadMeshSource(node);

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "id":
                                {
                                    source.ID = att.Value;

                                    break;
                                }
                            }
                        }

                        skin.Sources.Add(source);

                        break;
                    }
                    case "joints":
                    {
                        ControllerJoints joints = LoadControllerJoints(node);

                        skin.Joints = joints;

                        break;
                    }
                    case "vertex_weights":
                    {
                        ControllerVertexWeights weights = LoadControllerVertexWeights(node);

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "count":
                                {
                                    weights.Count = uint.Parse(att.Value);

                                    break;
                                }
                            }
                        }

                        skin.Weights = weights;

                        break;
                    }
                }
            }

            return skin;
        }

        Controller LoadController(XmlNode a_parentNode)
        {
            Controller controller = new Controller();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "skin":
                    {
                        ControllerSkin skin = LoadControllerSkin(node);

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "source":
                                {
                                    skin.Source = att.Value;

                                    break;
                                }
                            }
                        }

                        controller.Skin = skin;

                        break;
                    }
                }
            }

            return controller;
        }

        void LoadControllerLibrary(XmlNode a_parentNode)
        {
            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "controller":
                    {
                        Controller controller = LoadController(node);

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "id":
                                {
                                    controller.ID = att.Value;

                                    break;
                                }
                                case "name":
                                {
                                    controller.Name = att.Value;

                                    break; 
                                }
                            }
                        }

                        m_controller.Add(controller);

                        break;
                    }
                }
            }
        }

        SceneNode LoadSceneNode(XmlNode a_node, SceneNode a_parentSceneNode)
        {
            SceneNode sceneNode = new SceneNode();
            sceneNode.Children = new List<SceneNode>();

            foreach (XmlAttribute att in a_node.Attributes)
            {
                switch (att.Name)
                {
                    case "id":
                    {
                        sceneNode.ID = att.Value;

                        break;
                    }
                    case "name":
                    {
                        sceneNode.Name = att.Value;

                        break;
                    }
                    case "type":
                    {
                        sceneNode.Type = att.Value;

                        break;
                    }
                    case "sid":
                    {
                        sceneNode.SID = att.Value;

                        break;
                    }
                }
            }

            for (XmlNode node = a_node.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "matrix":
                    {
                        Matrix4 mat = Matrix4.Identity;

                        string str = node.InnerText;
                        string[] lines = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // TODO: Double check matrix orientation 
                        for (int x = 0; x < 4; ++x)
                        {
                            for (int y = 0; y < 4; ++y)
                            {
                                mat[x, y] = float.Parse(lines[x + (y * 4)]);
                            }
                        }

                        sceneNode.Transform = mat;

                        break;
                    }
                    case "node":
                    {
                        sceneNode.Children.Add(LoadSceneNode(node, sceneNode));

                        break;
                    }
                }
            }

            if (a_parentSceneNode != null)
            {
                sceneNode.Parent = a_parentSceneNode;
            }

            return sceneNode;
        }

        Scene LoadScene(XmlNode a_parentNode)
        {
            Scene scene = new Scene();
            scene.Nodes = new List<SceneNode>();

            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "node":
                    {
                        SceneNode sceneNode = LoadSceneNode(node, null);

                        scene.Nodes.Add(sceneNode);

                        break;
                    }
                }
            }

            return scene;
        }

        void LoadSceneLibrary(XmlNode a_parentNode)
        {
            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name)
                {
                    case "visual_scene":
                    {
                        Scene scene = LoadScene(node);

                        foreach (XmlAttribute att in node.Attributes)
                        {
                            switch (att.Name)
                            {
                                case "id":
                                {
                                    scene.ID = att.Value;

                                    break;
                                }
                                case "name":
                                {
                                    scene.Name = att.Value;

                                    break;
                                }
                            }
                        }

                        m_scene.Add(scene);

                        break;
                    }
                }
            }
        }

        void LoadData(XmlNode a_parentNode)
        {
            for (XmlNode node = a_parentNode.FirstChild; node != null; node = node.NextSibling)
            {
                switch (node.Name.ToLower())
                {
                    case "library_geometries":
                    {
                        LoadGeometryLibrary(node);

                        break;
                    }
                    case "library_controllers":
                    {
                        LoadControllerLibrary(node);

                        break;
                    }
                    case "library_visual_scenes":
                    {
                        LoadSceneLibrary(node);

                        break;
                    }
                }
            }
        }

        public ColladaLoader(Skeleton a_skeleton, string a_fileName, IFileSystem a_fileSystem)
        {
            m_skeleton = a_skeleton;

            m_geometry = new List<Geometry>();
            m_controller = new List<Controller>();
            m_scene = new List<Scene>();

            m_skeletonNodes = new List<SkeletonNode>();

            m_skeletonNameLookup = new Dictionary<string, SkeletonNode>();

            Stream stream;

            if (a_fileSystem.Load(a_fileName, out stream))
            {
                XmlDocument doc = new XmlDocument();

                doc.Load(stream);
                stream.Dispose();

                for (XmlNode node = doc.FirstChild; node != null; node = node.NextSibling)
                {
                    if (node.Name == "COLLADA")
                    {
                        LoadData(node);

                        break;
                    }
                }
            }
            else
            {
                InternalConsole.Warning("Failed to load: " + a_fileName);
            }
        }

        int[] GetValueOffset(List<MeshParam> a_params)
        {
            int len = a_params.Count;

            int[] values = new int[len];

            for (int i = 0; i < len; ++i)
            {
                switch (a_params[i].Name.ToUpper())
                {
                    case "X":
                    case "S":
                    {
                        values[i] = 0;

                        break;
                    }
                    case "Y":
                    case "T":
                    {
                        values[i] = 1;

                        break;
                    }
                    case "Z":
                    {
                        values[i] = 2;

                        break;
                    }
                    case "W":
                    {
                        values[i] = 3;

                        break;
                    }
                }
            }

            return values;
        }

        internal void GenerateModelData(int a_index, out Vertex[] a_vertices, out uint[] a_indices, out float a_length)
        {
            a_vertices = null;
            a_indices = null;
            a_length = 0;

            float len = 0;

            List<Vertex> vertices = new List<Vertex>();
            List<uint> indicies = new List<uint>();

            Dictionary<Vertex, uint> vertexLookup = new Dictionary<Vertex, uint>(); 

            // Find which mesh sources contain the data for positions, normals and texcoord
            Geometry geom = m_geometry[a_index];
            Mesh mesh = geom.Mesh;

            MeshVertex vertex = mesh.MeshVertex;
            MeshTriangle triangles = mesh.MeshTriangle;

            MeshSource positionData = new MeshSource();
            MeshSource normalData = new MeshSource();
            MeshSource texData = new MeshSource();

            Input positionInput = new Input();
            Input normalInput = new Input();
            Input texInput = new Input();

            int triangleStride = triangles.Inputs.Count;

            foreach (Input triInput in triangles.Inputs)
            {
                switch (triInput.Semantic.ToUpper())
                {
                    case "VERTEX":
                    {
                        if (triInput.Source == ("#" + vertex.ID))
                        {
                            foreach (Input vertInput in vertex.Inputs)
                            {
                                switch (vertInput.Semantic.ToUpper())
                                {
                                    case "POSITION":
                                    {
                                        foreach (MeshSource meshSource in mesh.MeshSources)
                                        {
                                            if (vertInput.Source == "#" + meshSource.ID)
                                            {
                                                positionData = meshSource;
                                                positionInput = triInput;

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                    case "NORMAL":
                                    {
                                        foreach (MeshSource meshSource in mesh.MeshSources)
                                        {
                                            if (vertInput.Source == "#" + meshSource.ID)
                                            {
                                                normalData = meshSource;
                                                normalInput = triInput;

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                    case "TEXCOORD":
                                    {
                                        foreach (MeshSource meshSource in mesh.MeshSources)
                                        {
                                            if (vertInput.Source == "#" + meshSource.ID)
                                            {
                                                texData = meshSource;
                                                texInput = triInput;

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                }
                            }
                        }

                        break;
                    }
                    case "POSITION":
                    {
                        foreach (MeshSource meshSource in mesh.MeshSources)
                        {
                            if (triInput.Source == "#" + meshSource.ID)
                            {
                                positionData = meshSource;
                                positionInput = triInput;

                                break;
                            }
                        }

                        break;
                    }
                    case "NORMAL":
                    {
                        foreach (MeshSource meshSource in mesh.MeshSources)
                        {
                            if (triInput.Source == "#" + meshSource.ID)
                            {
                                normalData = meshSource;
                                normalInput = triInput;

                                break;
                            }
                        }

                        break;
                    }
                    case "TEXCOORD":
                    {
                        foreach (MeshSource meshSource in mesh.MeshSources)
                        {
                            if (triInput.Source == "#" + meshSource.ID)
                            {
                                texData = meshSource;
                                texInput = triInput;

                                break;
                            }
                        }

                        break;
                    }
                }
            }

            MeshAccessor positionAccessor = positionData.Technique.Accessor;
            MeshAccessor normalAccessor = normalData.Technique.Accessor;
            MeshAccessor texAccessor = texData.Technique.Accessor;

            uint posCount = positionAccessor.Count;
            uint normCount = normalAccessor.Count;
            uint texCount = texAccessor.Count;

            // To convert vector offsets to xyzw instead of whatever the file uses
            int[] positionOffset = GetValueOffset(positionAccessor.Params);
            int[] normalOffset = GetValueOffset(normalAccessor.Params);
            int[] texOffset = GetValueOffset(texAccessor.Params);

            int posStride = positionAccessor.Stride;
            int normStride = normalAccessor.Stride;
            int texStride = texAccessor.Stride;

            // Turn the data into vectors for easier use
            Vector4[] positions = new Vector4[posCount];
            Vector3[] normals = new Vector3[normCount];
            Vector2[] texCoords = new Vector2[texCount];

            for (int i = 0; i < posCount; ++i)
            {
                positions[i] = new Vector4(0, 0, 0, 1);

                int index = i * posStride;

                for (int j = 0; j < posStride; ++j)
                {
                    positions[i][positionOffset[j]] = positionData.FloatData[index + j];
                }

                len = Math.Max(len, positions[i].LengthSquared);
            }

            for (int i = 0; i < normCount; ++i)
            {
                normals[i] = Vector3.Zero;

                int index = i * normStride;

                for (int j = 0; j < normStride; ++j)
                {
                    normals[i][normalOffset[j]] = normalData.FloatData[index + j];
                }
            }

            for (int i = 0; i < texCount; ++i)
            {
                texCoords[i] = Vector2.Zero;

                int index = i * texStride;

                for (int j = 0; j < texStride; ++j)
                {
                    texCoords[i][texOffset[j]] = texData.FloatData[index + j];
                }
            }

            uint triVertexCount = triangles.Count * 3;

            // Stitch Tris
            for (int i = 0; i < triVertexCount; ++i)
            {
                Vertex vert;

                int triIndex = i * triangleStride;

                uint posIndex = triangles.Data[triIndex + positionInput.Offset]; 
                uint normIndex = triangles.Data[triIndex + normalInput.Offset];
                uint texIndex = triangles.Data[triIndex + texInput.Offset];

                vert.Position = positions[posIndex];
                vert.Normal = normals[normIndex];
                vert.TexCoords = texCoords[texIndex];

                uint val = 0;

                if (!vertexLookup.TryGetValue(vert, out val))
                {
                    vertices.Add(vert);
                    val = (uint)(vertices.Count - 1);
                    vertexLookup.Add(vert, val);
                }

                indicies.Add(val);
            }

            a_vertices = vertices.ToArray();
            a_indices = indicies.ToArray();
            a_length = (float)Math.Sqrt(len);
        }

        public Model GetModel(int a_index, Pipeline a_pipeline)
        {
            Model model = new Model(a_pipeline);

            Vertex[] verts; 
            uint[] indices;
            float len;

            GenerateModelData(a_index, out verts, out indices, out len);

            model.Name = m_geometry[a_index].Name;

            model.SetModelData(verts, indices, len);

            return model;
        }

        public Model GetSkinnedModel(int a_index, Pipeline a_pipeline)
        {
            Model model = new Model(a_pipeline);

            float len = 0;

            List<SkinnedVertex> vertices = new List<SkinnedVertex>();
            List<uint> indicies = new List<uint>();

            Dictionary<SkinnedVertex, uint> vertexLookup = new Dictionary<SkinnedVertex, uint>(); 

            // Find which mesh sources contain the data for positions, normals and texcoord
            Geometry geom = m_geometry[a_index];
            model.Name = geom.Name;
            
            string meshHash = "#" + geom.ID;

            Mesh mesh = geom.Mesh;

            MeshVertex vertex = mesh.MeshVertex;
            MeshTriangle triangles = mesh.MeshTriangle;

            MeshSource positionData = new MeshSource();
            MeshSource normalData = new MeshSource();
            MeshSource texData = new MeshSource();
            MeshSource weightData = new MeshSource();
            MeshSource jointData = new MeshSource();

            Input vertexInput = new Input();
            Input positionInput = new Input();
            Input normalInput = new Input();
            Input texInput = new Input();
            Input jointInput = new Input();
            Input weightInput = new Input();

            uint weightInputs = 0;

            ControllerVertexWeights vertexWeights = new ControllerVertexWeights();

            uint triangleStride = (uint)triangles.Inputs.Count;

            foreach (Input triInput in triangles.Inputs)
            {
                switch (triInput.Semantic.ToUpper())
                {
                    case "VERTEX":
                    {
                        vertexInput = triInput;

                        foreach (Controller cont in m_controller)
                        {
                            ControllerSkin skin = cont.Skin;

                            if (skin.Source == meshHash)
                            {
                                vertexWeights = skin.Weights;

                                weightInputs = (uint)vertexWeights.Inputs.Count;

                                foreach (Input input in vertexWeights.Inputs)
                                {
                                    switch (input.Semantic.ToUpper())
                                    {
                                        case "JOINT":
                                        {
                                            foreach (MeshSource meshSource in skin.Sources)
                                            {
                                                if (input.Source == "#" + meshSource.ID)
                                                {
                                                    jointData = meshSource;
                                                    jointInput = input;

                                                    break;
                                                }    
                                            }

                                            break;
                                        }
                                        case "WEIGHT":
                                        {
                                            foreach (MeshSource meshSource in skin.Sources)
                                            {
                                                if (input.Source == "#" + meshSource.ID)
                                                {
                                                    weightData = meshSource;
                                                    weightInput = input;
                                                        
                                                    break;
                                                }
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (triInput.Source == "#" + vertex.ID)
                        {
                            foreach (Input vertInput in vertex.Inputs)
                            {
                                switch (vertInput.Semantic.ToUpper())
                                {
                                    case "POSITION":
                                    {
                                        foreach (MeshSource meshSource in mesh.MeshSources)
                                        {
                                            if (vertInput.Source == "#" + meshSource.ID)
                                            {
                                                positionData = meshSource;
                                                positionInput = triInput;

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                    case "NORMAL":
                                    {
                                        foreach (MeshSource meshSource in mesh.MeshSources)
                                        {
                                            if (vertInput.Source == "#" + meshSource.ID)
                                            {
                                                normalData = meshSource;
                                                normalInput = triInput;

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                    case "TEXCOORD":
                                    {
                                        foreach (MeshSource meshSource in mesh.MeshSources)
                                        {
                                            if (vertInput.Source == "#" + meshSource.ID)
                                            {
                                                texData = meshSource;
                                                texInput = triInput;

                                                break;
                                            }
                                        }

                                        break;
                                    }
                                }
                            }
                        }

                        break;
                    }
                    case "POSITION":
                    {
                        foreach (MeshSource meshSource in mesh.MeshSources)
                        {
                            if (triInput.Source == "#" + meshSource.ID)
                            {
                                positionData = meshSource;
                                positionInput = triInput;

                                break;
                            }
                        }

                        break;
                    }
                    case "NORMAL":
                    {
                        foreach (MeshSource meshSource in mesh.MeshSources)
                        {
                            if (triInput.Source == "#" + meshSource.ID)
                            {
                                normalData = meshSource;
                                normalInput = triInput;

                                break;
                            }
                        }

                        break;
                    }
                    case "TEXCOORD":
                    {
                        foreach (MeshSource meshSource in mesh.MeshSources)
                        {
                            if (triInput.Source == "#" + meshSource.ID)
                            {
                                texData = meshSource;
                                texInput = triInput;

                                break;
                            }
                        }

                        break;
                    }
                }
            }

            MeshAccessor positionAccessor = positionData.Technique.Accessor;
            MeshAccessor normalAccessor = normalData.Technique.Accessor;
            MeshAccessor texAccessor = texData.Technique.Accessor;

            uint posCount = positionAccessor.Count;
            uint normCount = normalAccessor.Count;
            uint texCount = texAccessor.Count;
            uint weightCount = vertexWeights.Count;

            // To convert vector offsets to xyzw instead of whatever the file uses
            int[] positionOffset = GetValueOffset(positionAccessor.Params);
            int[] normalOffset = GetValueOffset(normalAccessor.Params);
            int[] texOffset = GetValueOffset(texAccessor.Params);

            int posStride = positionAccessor.Stride;
            int normStride = normalAccessor.Stride;
            int texStride = texAccessor.Stride;

            // Turn the data into vectors for easier use
            Vector4[] positions = new Vector4[posCount];
            Vector3[] normals = new Vector3[normCount];
            Vector2[] texCoords = new Vector2[texCount];
            ushort[,] boneIndicies = new ushort[weightCount, 4];
            float[,] weights = new float[weightCount, 4];

            for (int i = 0; i < posCount; ++i)
            {
                positions[i] = new Vector4(0, 0, 0, 1);

                for (int j = 0; j < posStride; ++j)
                {
                    positions[i][positionOffset[j]] = positionData.FloatData[i * posStride + j];
                }

                len = Math.Max(len, positions[i].LengthSquared);
            }

            for (int i = 0; i < normCount; ++i)
            {
                normals[i] = Vector3.Zero;

                for (int j = 0; j < normStride; ++j)
                {
                    normals[i][normalOffset[j]] = normalData.FloatData[i * normStride + j];
                }
            }

            for (int i = 0; i < texCount; ++i)
            {
                texCoords[i] = Vector2.Zero;

                for (int j = 0; j < texStride; ++j)
                {
                    texCoords[i][texOffset[j]] = texData.FloatData[i * texStride + j];
                }
            }

            uint index = 0;
            for (uint i = 0; i < weightCount; ++i)
            {
                uint count = vertexWeights.WeightCount[i];
                for (int j = 0; j < count; ++j)
                {
                    uint lookUpIndex = vertexWeights.WeightBonePairs[index + jointInput.Offset];
                    string name = jointData.NameData[lookUpIndex];

                    ushort boneIndex = 0;
                    if (m_skeletonNameLookup.ContainsKey(name))
                    {
                        boneIndex = m_skeletonNameLookup[name].Index;
                    }
                    else
                    {
                        // This is stupid why do I have to take a index convert to a name then back to an index
                        // Why can it not use a name or index directly
                        SkeletonNode node = null;

                        if (m_skeleton != null)
                        {
                            node = m_skeleton.GetNode(name);

                            if (node == null)
                            {
                                // TODO: Fix index
                                node = new SkeletonNode((ushort)(m_skeleton.GetLastIndex() + 1), name);
                            }
                        }
                        else
                        {
                            node = new SkeletonNode((ushort)m_skeletonNodes.Count, name);
                        }

                        m_skeletonNameLookup.Add(name, node);
                        m_skeletonNodes.Add(node);

                        boneIndex = node.Index; 
                    }

                    boneIndicies[i, j] = boneIndex;
                    weights[i, j] = weightData.FloatData[vertexWeights.WeightBonePairs[index + weightInput.Offset]];

                    index += weightInputs;
                }

                for (uint j = count; j < 4; ++j)
                {
                    boneIndicies[i, j] = 0;
                    weights[i, j] = 0.0f;
                }
            }

            uint triVertexCount = triangles.Count * 3;

            // Stitch Tris
            for (uint i = 0; i < triVertexCount; ++i)
            {
                SkinnedVertex vert;    
                vert.Bones = Vector4.Zero;
                vert.Weights = Vector4.Zero;

                uint triIndex = i * triangleStride; 

                uint vertexData = triangles.Data[triIndex + vertexInput.Offset];
                for (int j = 0; j < 4; ++j)
                {
                    vert.Bones[j] = boneIndicies[vertexData, j] / 127.0f;
                    vert.Weights[j] = weights[vertexData, j];
                }

                uint posIndex = triangles.Data[triIndex + positionInput.Offset]; 
                uint normIndex = triangles.Data[triIndex + normalInput.Offset];
                uint texIndex = triangles.Data[triIndex + texInput.Offset];

                vert.Position = positions[posIndex];
                vert.Normal = normals[normIndex];
                vert.TexCoords = texCoords[texIndex];

                uint val = 0;

                if (!vertexLookup.TryGetValue(vert, out val))
                {
                    vertices.Add(vert);
                    val = (uint)(vertices.Count - 1);
                    vertexLookup.Add(vert, val);
                }

                indicies.Add(val);
            }

            model.SetModelData(vertices.ToArray(), indicies.ToArray(), (float)Math.Sqrt(len));

            return model;
        }

        SkeletonNode GenerateNode(SkeletonNode a_parent, SceneNode a_node)
        {
            SkeletonNode skeletonNode = null;

            if (a_node.Type.ToUpper() != "JOINT")
            {
                skeletonNode = a_parent;
            }
            else
            {
                string sid = a_node.SID;

                if (m_skeletonNameLookup.ContainsKey(sid))
                {
                    skeletonNode = m_skeletonNameLookup[sid];
                }
                else
                {
                    if (m_skeleton != null)
                    {
                        skeletonNode = m_skeleton.GetNode(sid);

                        if (skeletonNode == null)
                        {
                            skeletonNode = new SkeletonNode((ushort)(m_skeleton.GetLastIndex() + 1), sid);
                        }
                    }  
                    else
                    {
                        skeletonNode = new SkeletonNode((ushort)m_skeletonNodes.Count, sid);
                    }                    

                    skeletonNode.Transform = a_node.Transform;

                    m_skeletonNameLookup.Add(sid, skeletonNode);
                    m_skeletonNodes.Add(skeletonNode);
                }
            }

            foreach (SceneNode node in a_node.Children)
            {
                GenerateNode(skeletonNode, node);
            }

            skeletonNode.Parent = a_parent;

            return skeletonNode;
        }

        void GenerateNodeTree(ref SkeletonNode a_root, SceneNode a_node)
        {
            if (a_node.Type.ToUpper() == "JOINT")
            {
                a_root = GenerateNode(null, a_node);
            }
            else
            {
                foreach (SceneNode node in a_node.Children)
                {
                    GenerateNodeTree(ref a_root, node);

                    if (a_root != null)
                    {
                        return;
                    }
                }
            }
        }

        public Skeleton GetSkeleton()
        {
            SkeletonNode rootNode = null;

            foreach (Scene scene in m_scene)
            {
                if (scene.ID == "Scene")
                {
                    foreach (SceneNode node in scene.Nodes)
                    {
                        GenerateNodeTree(ref rootNode, node);

                        if (rootNode != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (m_skeleton == null)
            {
                m_skeleton = new Skeleton(rootNode);
            }

            return m_skeleton;
        }
    }
}