
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Barnacle.Object3DLib
{
    public partial class PrimitiveGenerator
    {
      public static void GenerateStaircase(ref Point3DCollection pnts, ref Int32Collection indices, ref Vector3DCollection normals)
      {
        double [] v =
        {
			-0.500,0.875,-0.500,
			-0.250,0.875,0.500,
			-0.500,0.875,0.500,
			-0.250,0.875,-0.500,
			-0.250,0.625,0.500,
			-0.250,0.625,-0.500,
			-0.249,0.617,0.500,
			-0.249,0.617,-0.500,
			-0.248,0.609,0.500,
			-0.248,0.609,-0.500,
			-0.245,0.601,0.500,
			-0.245,0.601,-0.500,
			-0.242,0.594,0.500,
			-0.242,0.594,-0.500,
			-0.237,0.587,0.500,
			-0.237,0.587,-0.500,
			-0.232,0.581,0.500,
			-0.232,0.581,-0.500,
			-0.226,0.575,0.500,
			-0.226,0.575,-0.500,
			-0.219,0.571,0.500,
			-0.219,0.571,-0.500,
			-0.211,0.567,0.500,
			-0.211,0.567,-0.500,
			-0.204,0.565,0.500,
			-0.204,0.565,-0.500,
			-0.196,0.563,0.500,
			-0.196,0.563,-0.500,
			-0.188,0.563,0.500,
			-0.188,0.563,-0.500,
			0.063,0.563,0.500,
			0.063,0.563,-0.500,
			0.063,0.313,0.500,
			0.063,0.313,-0.500,
			0.063,0.304,0.500,
			0.063,0.304,-0.500,
			0.065,0.296,0.500,
			0.065,0.296,-0.500,
			0.067,0.289,0.500,
			0.067,0.289,-0.500,
			0.071,0.281,0.500,
			0.071,0.281,-0.500,
			0.075,0.274,0.500,
			0.075,0.274,-0.500,
			0.081,0.268,0.500,
			0.081,0.268,-0.500,
			0.087,0.263,0.500,
			0.087,0.263,-0.500,
			0.094,0.258,0.500,
			0.094,0.258,-0.500,
			0.101,0.255,0.500,
			0.101,0.255,-0.500,
			0.109,0.252,0.500,
			0.109,0.252,-0.500,
			0.117,0.251,0.500,
			0.117,0.251,-0.500,
			0.125,0.250,0.500,
			0.125,0.250,-0.500,
			0.375,0.250,0.500,
			0.375,0.250,-0.500,
			0.375,0.000,0.500,
			0.375,0.000,-0.500,
			0.500,0.000,0.500,
			0.500,0.000,-0.500,
			0.500,0.313,0.500,
			0.500,0.313,-0.500,
			0.499,0.321,0.500,
			0.499,0.321,-0.500,
			0.498,0.329,0.500,
			0.498,0.329,-0.500,
			0.495,0.336,0.500,
			0.495,0.336,-0.500,
			0.492,0.344,0.500,
			0.492,0.344,-0.500,
			0.487,0.351,0.500,
			0.487,0.351,-0.500,
			0.482,0.357,0.500,
			0.482,0.357,-0.500,
			0.476,0.362,0.500,
			0.476,0.362,-0.500,
			0.469,0.367,0.500,
			0.469,0.367,-0.500,
			0.461,0.370,0.500,
			0.461,0.370,-0.500,
			0.454,0.373,0.500,
			0.454,0.373,-0.500,
			0.446,0.374,0.500,
			0.446,0.374,-0.500,
			0.438,0.375,0.500,
			0.438,0.375,-0.500,
			0.188,0.375,0.500,
			0.188,0.375,-0.500,
			0.188,0.625,0.500,
			0.188,0.625,-0.500,
			0.187,0.633,0.500,
			0.187,0.633,-0.500,
			0.185,0.641,0.500,
			0.185,0.641,-0.500,
			0.183,0.649,0.500,
			0.183,0.649,-0.500,
			0.179,0.656,0.500,
			0.179,0.656,-0.500,
			0.175,0.663,0.500,
			0.175,0.663,-0.500,
			0.169,0.669,0.500,
			0.169,0.669,-0.500,
			0.163,0.675,0.500,
			0.163,0.675,-0.500,
			0.156,0.679,0.500,
			0.156,0.679,-0.500,
			0.149,0.683,0.500,
			0.149,0.683,-0.500,
			0.141,0.685,0.500,
			0.141,0.685,-0.500,
			0.133,0.687,0.500,
			0.133,0.687,-0.500,
			0.125,0.688,0.500,
			0.125,0.688,-0.500,
			-0.125,0.688,0.500,
			-0.125,0.688,-0.500,
			-0.125,0.938,0.500,
			-0.125,0.938,-0.500,
			-0.126,0.946,0.500,
			-0.126,0.946,-0.500,
			-0.127,0.954,0.500,
			-0.127,0.954,-0.500,
			-0.130,0.961,0.500,
			-0.130,0.961,-0.500,
			-0.133,0.969,0.500,
			-0.133,0.969,-0.500,
			-0.138,0.976,0.500,
			-0.138,0.976,-0.500,
			-0.143,0.982,0.500,
			-0.143,0.982,-0.500,
			-0.149,0.987,0.500,
			-0.149,0.987,-0.500,
			-0.156,0.992,0.500,
			-0.156,0.992,-0.500,
			-0.164,0.995,0.500,
			-0.164,0.995,-0.500,
			-0.171,0.998,0.500,
			-0.171,0.998,-0.500,
			-0.179,0.999,0.500,
			-0.179,0.999,-0.500,
			-0.188,1.000,0.500,
			-0.188,1.000,-0.500,
			-0.500,1.000,0.500,
			-0.500,1.000,-0.500,
			-0.249,0.617,-0.500,
			-0.249,0.617,0.500,
			-0.248,0.609,-0.500,
			-0.248,0.609,0.500,
			-0.242,0.594,-0.500,
			-0.242,0.594,0.500,
			0.063,0.304,-0.500,
			0.063,0.304,0.500,
			0.067,0.289,-0.500,
			0.067,0.289,0.500,
			0.075,0.274,-0.500,
			0.075,0.274,0.500,
			0.081,0.268,-0.500,
			0.081,0.268,0.500,
			0.087,0.263,-0.500,
			0.087,0.263,0.500,
			0.117,0.251,-0.500,
			0.117,0.251,0.500,
			0.499,0.321,-0.500,
			0.499,0.321,0.500,
			0.492,0.344,-0.500,
			0.492,0.344,0.500,
			0.487,0.351,-0.500,
			0.487,0.351,0.500,
			0.482,0.357,-0.500,
			0.482,0.357,0.500,
			0.476,0.362,-0.500,
			0.476,0.362,0.500,
			0.187,0.633,-0.500,
			0.187,0.633,0.500,
			0.175,0.663,-0.500,
			0.175,0.663,0.500,
			0.169,0.669,-0.500,
			0.169,0.669,0.500,
			0.163,0.675,-0.500,
			0.163,0.675,0.500,
			0.133,0.687,-0.500,
			0.133,0.687,0.500,
			-0.127,0.954,-0.500,
			-0.127,0.954,0.500,
			-0.133,0.969,-0.500,
			-0.133,0.969,0.500,
			-0.138,0.976,-0.500,
			-0.138,0.976,0.500,
			-0.164,0.995,-0.500,
			-0.164,0.995,0.500,
			-0.171,0.998,-0.500,
			-0.171,0.998,0.500,

        };

        int [] f =
        {
			0,1,2,
			0,3,1,
			3,4,1,
			3,5,4,
			5,6,4,
			5,7,6,
			7,8,6,
			7,9,8,
			9,10,8,
			9,11,10,
			11,12,10,
			11,13,12,
			13,14,12,
			13,15,14,
			15,16,14,
			15,17,16,
			17,18,16,
			17,19,18,
			19,20,18,
			19,21,20,
			21,22,20,
			21,23,22,
			23,24,22,
			23,25,24,
			25,26,24,
			25,27,26,
			27,28,26,
			27,29,28,
			29,30,28,
			29,31,30,
			31,32,30,
			31,33,32,
			33,34,32,
			33,35,34,
			35,36,34,
			35,37,36,
			37,38,36,
			37,39,38,
			39,40,38,
			39,41,40,
			41,42,40,
			41,43,42,
			43,44,42,
			43,45,44,
			45,46,44,
			45,47,46,
			47,48,46,
			47,49,48,
			49,50,48,
			49,51,50,
			51,52,50,
			51,53,52,
			53,54,52,
			53,55,54,
			55,56,54,
			55,57,56,
			57,58,56,
			57,59,58,
			59,60,58,
			59,61,60,
			61,62,60,
			61,63,62,
			63,64,62,
			63,65,64,
			65,66,64,
			65,67,66,
			67,68,66,
			67,69,68,
			69,70,68,
			69,71,70,
			71,72,70,
			71,73,72,
			73,74,72,
			73,75,74,
			75,76,74,
			75,77,76,
			77,78,76,
			77,79,78,
			79,80,78,
			79,81,80,
			81,82,80,
			81,83,82,
			83,84,82,
			83,85,84,
			85,86,84,
			85,87,86,
			87,88,86,
			87,89,88,
			89,90,88,
			89,91,90,
			91,92,90,
			91,93,92,
			93,94,92,
			93,95,94,
			95,96,94,
			95,97,96,
			97,98,96,
			97,99,98,
			99,100,98,
			99,101,100,
			101,102,100,
			101,103,102,
			103,104,102,
			103,105,104,
			105,106,104,
			105,107,106,
			107,108,106,
			107,109,108,
			109,110,108,
			109,111,110,
			111,112,110,
			111,113,112,
			113,114,112,
			113,115,114,
			115,116,114,
			115,117,116,
			117,118,116,
			117,119,118,
			119,120,118,
			119,121,120,
			121,122,120,
			121,123,122,
			123,124,122,
			123,125,124,
			125,126,124,
			125,127,126,
			127,128,126,
			127,129,128,
			129,130,128,
			129,131,130,
			131,132,130,
			131,133,132,
			133,134,132,
			133,135,134,
			135,136,134,
			135,137,136,
			137,138,136,
			137,139,138,
			139,140,138,
			139,141,140,
			141,142,140,
			141,143,142,
			143,144,142,
			143,145,144,
			145,146,144,
			145,147,146,
			147,2,146,
			147,0,2,
			3,148,5,
			1,4,149,
			3,150,148,
			1,149,151,
			3,11,150,
			1,151,10,
			3,152,11,
			1,10,153,
			3,15,152,
			1,153,14,
			3,17,15,
			1,14,16,
			3,19,17,
			1,16,18,
			3,21,19,
			1,18,20,
			3,23,21,
			1,20,22,
			3,25,23,
			1,22,24,
			3,27,25,
			1,24,26,
			3,29,27,
			1,26,28,
			31,154,33,
			30,32,155,
			31,37,154,
			30,155,36,
			31,156,37,
			30,36,157,
			31,41,156,
			30,157,40,
			31,158,41,
			30,40,159,
			31,160,158,
			30,159,161,
			31,162,160,
			30,161,163,
			31,49,162,
			30,163,48,
			31,51,49,
			30,48,50,
			31,53,51,
			30,50,52,
			31,164,53,
			30,52,165,
			31,57,164,
			30,165,56,
			59,63,61,
			58,60,62,
			59,65,63,
			58,62,64,
			57,65,59,
			56,58,64,
			57,166,65,
			56,64,167,
			57,69,166,
			56,167,68,
			57,71,69,
			56,68,70,
			57,168,71,
			56,70,169,
			57,170,168,
			56,169,171,
			57,172,170,
			56,171,173,
			57,174,172,
			56,173,175,
			57,81,174,
			56,175,80,
			57,83,81,
			56,80,82,
			57,85,83,
			56,82,84,
			57,87,85,
			56,84,86,
			57,89,87,
			56,86,88,
			57,91,89,
			56,88,90,
			31,91,57,
			30,56,90,
			31,93,91,
			30,90,92,
			29,93,31,
			28,30,92,
			29,176,93,
			28,92,177,
			29,97,176,
			28,177,96,
			29,99,97,
			28,96,98,
			29,101,99,
			28,98,100,
			29,178,101,
			28,100,179,
			29,180,178,
			28,179,181,
			29,182,180,
			28,181,183,
			29,109,182,
			28,183,108,
			29,111,109,
			28,108,110,
			29,113,111,
			28,110,112,
			29,184,113,
			28,112,185,
			29,117,184,
			28,185,116,
			29,119,117,
			28,116,118,
			3,119,29,
			1,28,118,
			3,121,119,
			1,118,120,
			0,121,3,
			2,1,120,
			0,123,121,
			2,120,122,
			0,186,123,
			2,122,187,
			0,127,186,
			2,187,126,
			0,188,127,
			2,126,189,
			0,190,188,
			2,189,191,
			0,133,190,
			2,191,132,
			0,135,133,
			2,132,134,
			0,137,135,
			2,134,136,
			0,192,137,
			2,136,193,
			0,194,192,
			2,193,195,
			0,143,194,
			2,195,142,
			0,145,143,
			2,142,144,
			0,147,145,
			2,144,146,

        };

        BuildPrimitive(pnts, indices, v, f);
      }
    }
}