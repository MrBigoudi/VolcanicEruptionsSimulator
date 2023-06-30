#pragma once

static const float H = 0.5f;
static const float PI = 3.1416f;

static const float ALPHA_POLY6 = 4.0f / (PI * H * H * H * H * H * H * H * H);
static const float ALPHA_POLY6_LAPLACIAN = 32.0f / (PI * H * H * H * H * H * H * H);
static const float ALPHA_VISCOSITY = 10.0f / (9.0f * PI * H * H * H * H * H);
static const float ALPHA_VISCOSITY_LAPLACIAN = 40.0f / (PI * H * H * H * H * H);