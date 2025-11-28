using RoR2;
using System.Reflection;
using UnityEngine;

namespace AncientScepter
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

            Material mat = UnityEngine.Object.Instantiate<Material>(commandoMat);
            Material tempMat = Assets.mainAssetBundle.LoadAsset<Material>(materialName);
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

        public static Material CreateMaterial(string materialName)
        {
            return Assets.CreateMaterial(materialName, 0f);
        }

        public static Material CreateMaterial(string materialName, float emission)
        {
            return Assets.CreateMaterial(materialName, emission, Color.black);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor)
        {
            return Assets.CreateMaterial(materialName, emission, emissionColor, 0f);
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

        public static class SpriteAssets
        {
            public static Sprite ArtificerFlameThrower2;
            public static Sprite ArtificerFlyUp2;
            public static Sprite Bandit2ResetRevolver2;
            public static Sprite Bandit2SkullRevolver2;
            public static Sprite CaptainAirstrike2;
            public static Sprite CaptainAirstrikeAlt2;
            public static Sprite CommandoBarrage2;
            public static Sprite CommandoGrenade2;
            public static Sprite CrocoDisease2;
            public static Sprite EngiTurret2;
            public static Sprite EngiWalker2;
            public static Sprite HereticNevermore2;
            public static Sprite HuntressBallista2;
            public static Sprite HuntressRain2;
            public static Sprite LoaderChargeFist2;
            public static Sprite LoaderChargeZapFist2;
            public static Sprite MercEvis2;
            public static Sprite MercEvis2Projectile;
            public static Sprite ToolbotDash2;
            public static Sprite TreebotFireFruitSeed2;
            public static Sprite TreebotFlower2_2;
            public static Sprite RailgunnerSupercharge2;
            public static Sprite RailgunnerFireSupercharge2;
            public static Sprite RailgunnerCryocharge2;
            public static Sprite RailgunnerFireCryocharge2;
            public static Sprite VoidFiendSuppress2;
            public static Sprite VoidFiendCorruptedSuppress2;
            public static Sprite SeekerMeditate2;
            public static Sprite SeekerPalmBlast2;
            public static Sprite ChefGlaze2;
            public static Sprite ChefYesChef2;
            public static Sprite DrifterSalvage2;
            public static Sprite DrifterTinker2;

            public static void InitializeAssets()
            {
                ArtificerFlameThrower2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texArtiR1");
                ArtificerFlyUp2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texArtiR2");
                Bandit2ResetRevolver2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texBanditR1");
                Bandit2SkullRevolver2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texBanditR2");
                CaptainAirstrike2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texCapU1");
                CaptainAirstrikeAlt2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texCapU2");
                CommandoBarrage2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texCommandoR1");
                CommandoGrenade2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texCommandoR2");
                CrocoDisease2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texAcridR1");
                EngiTurret2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texEngiR1");
                EngiWalker2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texEngiR2"); ;
                HereticNevermore2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texHereticR2");
                HuntressBallista2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texHuntressR2");
                HuntressRain2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texHuntressR1");
                LoaderChargeFist2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texLoaderU1");
                LoaderChargeZapFist2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texLoaderU2");
                MercEvis2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texMercR1");
                MercEvis2Projectile = Assets.mainAssetBundle.LoadAsset<Sprite>("texMercR2");
                ToolbotDash2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texMultU1");
                TreebotFireFruitSeed2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texRexR2");
                TreebotFlower2_2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texRexR1");
                RailgunnerSupercharge2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texRailgunnerR1");
                RailgunnerFireSupercharge2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texRailgunnerP1");
                RailgunnerCryocharge2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texRailgunnerR2");
                RailgunnerFireCryocharge2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texRailgunnerP2");
                VoidFiendSuppress2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texVoidFiendR1");
                VoidFiendCorruptedSuppress2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texVoidFiendR1C");
                SeekerMeditate2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texSeekerR1");
                SeekerPalmBlast2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texSeekerR2");
                ChefGlaze2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texChefR1");
                ChefYesChef2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texChefR2");
                DrifterSalvage2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texDrifterR1");
                DrifterTinker2 = Assets.mainAssetBundle.LoadAsset<Sprite>("texDrifterR2");
            }
        }
    }
}