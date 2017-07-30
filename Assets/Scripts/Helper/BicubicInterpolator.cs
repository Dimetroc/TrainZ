namespace Helper
{
    public struct BicubicInterpolator
    {
        private readonly float _a00;
        private readonly float _a01;
        private readonly float _a02;
        private readonly float _a03;
        private readonly float _a10;
        private readonly float _a11;
        private readonly float _a12;
        private readonly float _a13;
        private readonly float _a20;
        private readonly float _a21;
        private readonly float _a22;
        private readonly float _a23;
        private readonly float _a30;
        private readonly float _a31;
        private readonly float _a32;
        private readonly float _a33;

        public BicubicInterpolator(float[,] p)
        {
            _a00 = p[1, 1];
            _a01 = -0.5f*p[1, 0] + 0.5f*p[1, 2];
            _a02 = p[1, 0] - 2.5f*p[1, 1] + 2.0f*p[1, 2] - 0.5f*p[1, 3];
            _a03 = -0.5f*p[1, 0] + 1.5f*p[1, 1] - 1.5f*p[1, 2] + 0.5f*p[1, 3];
            _a10 = -0.5f*p[0, 1] + 0.5f*p[2, 1];
            _a11 = 0.25f*p[0, 0] - 0.25f*p[0, 2] - 0.25f*p[2, 0] + 0.25f*p[2, 2];
            _a12 = -0.5f*p[0, 0] + 1.25f*p[0, 1] - p[0, 2] + 0.25f*p[0, 3] + 0.5f*p[2, 0] - 1.25f*p[2, 1] + p[2, 2] - 0.25f*p[2, 3];
            _a13 = 0.25f*p[0, 0] - 0.75f*p[0, 1] + 0.75f*p[0, 2] - 0.25f*p[0, 3] - 0.25f*p[2, 0] + 0.75f*p[2, 1] - 0.75f*p[2, 2] + 0.25f*p[2, 3];
            _a20 = p[0, 1] - 2.5f*p[1, 1] + 2.0f*p[2, 1] - 0.5f*p[3, 1];
            _a21 = -0.5f*p[0, 0] + 0.5f*p[0, 2] + 1.25f*p[1, 0] - 1.25f*p[1, 2] - p[2, 0] + p[2, 2] + 0.25f*p[3, 0] - 0.25f*p[3, 2];
            _a22 = p[0, 0] - 2.5f*p[0, 1] + 2.0f*p[0, 2] - 0.5f*p[0, 3] - 2.5f*p[1, 0] + 6.25f*p[1, 1] - 5.0f*p[1, 2] + 1.25f*p[1, 3] + 2.0f*p[2, 0] - 5.0f*p[2, 1] + 4.0f*p[2, 2] - p[2, 3] - 0.5f*p[3, 0] + 1.25f*p[3, 1] - p[3, 2] +
                   0.25f*p[3, 3];
            _a23 = -0.5f*p[0, 0] + 1.5f*p[0, 1] - 1.5f*p[0, 2] + 0.5f*p[0, 3] + 1.25f*p[1, 0] - 3.75f*p[1, 1] + 3.75f*p[1, 2] - 1.25f*p[1, 3] - p[2, 0] + 3.0f*p[2, 1] - 3.0f*p[2, 2] + p[2, 3] + 0.25f*p[3, 0] - 0.75f*p[3, 1] + 0.75f*p[3, 2] -
                   0.25f*p[3, 3];
            _a30 = -0.5f*p[0, 1] + 1.5f*p[1, 1] - 1.5f*p[2, 1] + 0.5f*p[3, 1];
            _a31 = 0.25f*p[0, 0] - 0.25f*p[0, 2] - 0.75f*p[1, 0] + 0.75f*p[1, 2] + 0.75f*p[2, 0] - 0.75f*p[2, 2] - 0.25f*p[3, 0] + 0.25f*p[3, 2];
            _a32 = -0.5f*p[0, 0] + 1.25f*p[0, 1] - p[0, 2] + 0.25f*p[0, 3] + 1.5f*p[1, 0] - 3.75f*p[1, 1] + 3.0f*p[1, 2] - 0.75f*p[1, 3] - 1.5f*p[2, 0] + 3.75f*p[2, 1] - 3.0f*p[2, 2] + 0.75f*p[2, 3] + 0.5f*p[3, 0] - 1.25f*p[3, 1] + p[3, 2] -
                   0.25f*p[3, 3];
            _a33 = 0.25f*p[0, 0] - 0.75f*p[0, 1] + 0.75f*p[0, 2] - 0.25f*p[0, 3] - 0.75f*p[1, 0] + 2.25f*p[1, 1] - 2.25f*p[1, 2] + 0.75f*p[1, 3] + 0.75f*p[2, 0] - 2.25f*p[2, 1] + 2.25f*p[2, 2] - 0.75f*p[2, 3] - 0.25f*p[3, 0] + 0.75f*p[3, 1] -
                   0.75f*p[3, 2] + 0.25f*p[3, 3];
        }

        public float GetValue(float x, float y)
        {
            float x2 = x*x, x3 = x2*x, y2 = y*y, y3 = y2*y;
            return (_a00 + _a01*y + _a02*y2 + _a03*y3) + (_a10 + _a11*y + _a12*y2 + _a13*y3)*x + (_a20 + _a21*y + _a22*y2 + _a23*y3)*x2 + (_a30 + _a31*y + _a32*y2 + _a33*y3)*x3;
        }
    }
}
