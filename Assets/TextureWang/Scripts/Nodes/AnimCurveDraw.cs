using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Source/AnimCurveDraw")]
    public class AnimCurveDraw : CreateOp
    {
        public const string ID = "AnimCurveDraw";
        public override string GetID { get { return ID; } }

        public AnimationCurve m_RemapCurve=new AnimationCurve();
        public AnimationCurve m_RemapCurveY = new AnimationCurve();
        public FloatRemap m_RepeatX= new FloatRemap(1.0f,0,10);
        public FloatRemap m_RepeatY = new FloatRemap(1.0f,0,1);
        public FloatRemap m_OffsetY = new FloatRemap(0.5f, 0, 1);
        public FloatRemap m_Mirror = new FloatRemap(0.0f, 0, 1);

        //public Texture m_Cached;


        protected internal override void InspectorNodeGUI()
        {
        
        }

        public override Node Create (Vector2 _pos) 
        {

            AnimCurveDraw node = CreateInstance<AnimCurveDraw> ();
        
            node.rect = new Rect(_pos.x, _pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "AnimCurveDraw";
        
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            return node;
        }

        private void OnGUI()
        {
            NodeGUI();
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            //m_TexMode = (TexMode)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Colors", "3 components per texture or one"), m_TexMode, GUILayout.MaxWidth(200));

//        m_Value1 = RTEditorGUI.Slider(m_Value1, -100.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            m_RemapCurve = EditorGUILayout.CurveField(m_RemapCurve);
            m_RemapCurveY = EditorGUILayout.CurveField(m_RemapCurveY);
            m_RepeatX.SliderLabel(this, "RepeatX");
            m_RepeatY.SliderLabel(this, "ScaleY"); 
            m_OffsetY.SliderLabel(this, "OffsetY");
            m_Mirror.SliderLabel(this, ">0.5 is Mirror");

        }
        protected Texture2D gradientX;// = new Texture2D(256, 1, TextureFormat.ARGB32, false);
//    protected Texture2D gradientY;// = new Texture2D(256, 1, TextureFormat.ARGB32, false);
        public void ExecuteRemapCurve(float _size,  TextureParam _output)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            Material mat = GetMaterial("TextureOps");
            mat.SetInt("_MainIsGrey", 1);//_input.IsGrey() ? 1 : 0);
            mat.SetInt("_TextureBIsGrey", 1);
            mat.SetVector("_Multiply",new Vector4(m_RepeatX,m_RepeatY,m_OffsetY,m_Mirror));

            AnimCurveToTexture(ref gradientX, m_RemapCurve);
//        AnimCurveToTexture(ref gradientY, m_RemapCurveY);


//        mat.SetTexture("_GradientTex", gradientY);
//        mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_input.m_Width, 1.0f / (float)_input.m_Height, _size, 0));
            m_TexMode=TexMode.Greyscale;
            RenderTexture destination = _output.CreateRenderDestination(m_TexWidth, m_TexHeight, TextureParam.GetRTFormat(m_TexMode == TexMode.Greyscale, m_PixelDepth));
            SetCommonVars(mat);
            Graphics.Blit(gradientX, destination, mat, (int)ShaderOp.AnimCurveDraw);


//        Debug.LogError(" multiply in Final" + timer.ElapsedMilliseconds + " ms");
        }

        private void AnimCurveToTexture(ref Texture2D gradient, AnimationCurve _curve)
        {
            if (gradient == null)
                gradient = new Texture2D(2048, 1, TextureFormat.RFloat, false,true); //2048 possible levels

            Color c = Color.white;
            for (float x = 0.0f; x < (float) gradient.width; x += 1.0f)
            {
                c.g = c.b = c.r = _curve.Evaluate(x/(float) gradient.width);
//            if(c.r>=0.99f)
//                Debug.Log("reaches 1 at coord "+x+" c  "+c+" gamma "+c.gamma+" lin "+c.linear);
            
                gradient.SetPixel((int) x, 0, c);
            }
        
            gradient.wrapMode = TextureWrapMode.Repeat;
            gradient.Apply();
        }


        public override bool Calculate()
        {
            if (!allInputsReady())
            {
                //Debug.LogError(" input no ready");
                return false;
            }
            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth,m_TexHeight);
        

            if ( m_Param != null)
            {
                ExecuteRemapCurve(m_Value1,  m_Param);

            }
            CreateCachedTextureIcon();
            //m_Cached = m_Param.GetHWSourceTexture();
            m_Param.GetHWSourceTexture().wrapMode=TextureWrapMode.Clamp;
            Outputs[0].SetValue<TextureParam> (m_Param);
            
            return true;
        }
    }
}