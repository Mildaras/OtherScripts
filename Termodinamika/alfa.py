import numpy as np

# -------------------------------------------------------------------------------
# 1) Apibrėžiame funkciją f(α, Kp), kurios šaknį (f=0) norime rasti:
# -------------------------------------------------------------------------------
def f_alpha(alpha, Kp):
    # Ieškosime α intervale [-0.9999, 0.9999]:
    if alpha <= -1.0 or alpha >= 1.0:
        return np.inf

    denom = 5.0 - alpha
    if denom <= 0:
        return np.inf

    num1 = (2.0 + 2.0 * alpha) / denom       # (2 + 2α)/(5 − α)
    num2 = (2.0 - 2.0 * alpha) / denom       # (2 − 2α)/(5 − α)
    num3 = (1.0 - alpha) / denom             # (1 − α)/(5 − α)

    if num3 < 0:
        return np.inf

    lhs = num1 / (num2 * np.sqrt(num3))
    return lhs - Kp


# -------------------------------------------------------------------------------
# 2) Implementuojame bisection metodą su NumPy (be scipy):
# -------------------------------------------------------------------------------
def find_alpha_bisection(Kp, x_low=-0.9999, x_high=0.9999, tol=1e-8, max_iter=100):
    f_low  = f_alpha(x_low,  Kp)
    f_high = f_alpha(x_high, Kp)

    if np.isinf(f_low) or np.isinf(f_high):
        return np.nan
    if f_low * f_high > 0:
        return np.nan

    a  = x_low
    b  = x_high
    fa = f_low
    fb = f_high

    for _ in range(max_iter):
        c  = 0.5 * (a + b)
        fc = f_alpha(c, Kp)

        if np.abs(fc) < 1e-12 or (b - a) < tol:
            return c

        if fa * fc <= 0:
            b  = c
            fb = fc
        else:
            a  = c
            fa = fc

    return 0.5 * (a + b)


# -------------------------------------------------------------------------------
# 3) Pavyzdiniai Kp_list ir atitinkamos T vertės (įrašykite savo duomenis):
# -------------------------------------------------------------------------------
Kp_list = np.array([0.0035, 0.0449, 0.4712, 4.1111, 30.4504, 194.9416], dtype=float)
T_list  = np.array([230,    240,    250,    260,    270,      280    ], dtype=float)

if Kp_list.shape != T_list.shape:
    raise ValueError("Kp_list ir T_list dydžiai nesutampa!")

# -------------------------------------------------------------------------------
# 4) Randame α kiekvienam Kp:
# -------------------------------------------------------------------------------
alpha_list = np.empty_like(Kp_list)
for i, Kp in enumerate(Kp_list):
    alpha_list[i] = find_alpha_bisection(Kp)

# -------------------------------------------------------------------------------
# 5) Išvedame lentelę su Temperatūra, Kp ir α:
# -------------------------------------------------------------------------------
print("Temperatūra (K) |    Kp     |      α")
print("---------------+-----------+------------")

for T, Kp, α in zip(T_list, Kp_list, alpha_list):
    print(f"   {T:7.1f}    | {Kp:8.4f} |   {α:10.6f}")

# -------------------------------------------------------------------------------
# 6) Išvedame α reikšmes kaip Python list literal copy-paste’ui:
# -------------------------------------------------------------------------------
formatted_alphas = "[" + ", ".join(f"{float(v):.6f}" for v in alpha_list) + "]"
print("\nAlpha list for copy-paste:")
print(formatted_alphas)
