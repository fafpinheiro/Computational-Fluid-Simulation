# FluidSimulation-MasterThesis

Implementation of the SPH Algorithm (fluid simulation) in Unity


## Navier–Stokes Equations

The Navier–Stokes equations describe the motion of viscous fluids.

### Continuity Equation (Mass Conservation)

$$
\nabla \cdot \mathbf{u} = 0
$$

---

### Momentum Equation

∂t∂u​+(u⋅∇)u=−ρ1​∇p+ν∇2u+f

---

### Variables

| Symbol | Meaning |
|--------|--------|
| $\mathbf{u}$ | Velocity field |
| $t$ | Time |
| $\rho$ | Fluid density |
| $p$ | Pressure |
| $\nu$ | Kinematic viscosity |
| $\mathbf{f}$ | External forces |

---

### Expanded 3D Form

∂u/∂t + u∂u/∂x  + v∂u/∂y  + w∂u/∂z  = -1/ρ ∂p/∂x  + ν∇²u  + fₓ<br>

∂v/∂t + u∂v/∂x  + v∂v/∂y  + w∂v/∂z  = -1/ρ ∂p/∂y  + ν∇²v  + fᵧ<br>
<br>​​​
∂w/∂t + u∂w/∂x  + v∂w/∂y  + w∂w/∂z  = -1/ρ ∂p/∂z  + ν∇²w  + fz<br>


