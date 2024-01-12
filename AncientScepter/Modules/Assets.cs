using RoR2;
using System.Reflection;
using UnityEngine;

namespace AncientScepter.Modules
{
    public static class Assets
    {
        internal static AssetBundle mainAssetBundle;

        public static Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/HGStandard");
        public static Material commandoMat;

        public static void PopulateAssets()
        {
            if (mainAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AncientScepter.ancientscepter"))
                {
                    mainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                }
            }
        }

        // code for creating materials w hopoo shader-
        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!commandoMat) commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            Material mat = Object.Instantiate(commandoMat);
            Material tempMat = mainAssetBundle.LoadAsset<Material>(materialName);
            if (!tempMat)
            {
                return commandoMat;
            }

            mat.name = materialName;
            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            return mat;
        }

        public static CharacterModel.RendererInfo[] SetupRendererInfos(GameObject obj)
        {
            Renderer[] meshes = obj.GetComponentsInChildren<Renderer>();
            CharacterModel.RendererInfo[] rendererInfos = new CharacterModel.RendererInfo[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
            {
                rendererInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = meshes[i].material,
                    renderer = meshes[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                };
            }

            return rendererInfos;
        }
    }
}