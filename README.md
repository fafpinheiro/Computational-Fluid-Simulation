#     Computational-Fluid-Simulation


## Fluid Simulation using SPH in Unity

This project implements a real-time fluid simulation environment based on the Smoothed Particle Hydrodynamics (SPH) method.

The main goal was not visual realism, but to achieve:

- Physically plausible behaviour

- Performance for real-time projects

- Algorithmic simplicity

- Numerical stability

This makes the project focused on simulation quality and efficiency, rather than graphics.

---

## Approach

The simulation follows a Lagrangian particle-based model, where fluid is represented as a system of interacting particles governed by SPH equations.

A baseline implementation (“Default Method”) was used as the starting point and then improved through several optimization strategies.

##Implemented Optimizations

To improve computational efficiency while maintaining accuracy, the following techniques were developed:

- Force Symmetry
Reduces redundant calculations by enforcing physical reciprocity between particle interactions.

 -Monte Carlo Sampling
Limits neighborhood evaluation cost while preserving statistical correctness.

- Spatial Hash Grid (Hybrid SPH)
Accelerates neighbor search, significantly reducing simulation complexity.

---

## Outcome

The result is a modular and performant CFD-oriented simulation system capable of reproducing realistic fluid behavior under different interaction scenarios, including obstacle collision and dynamic flow conditions.

The project also includes:

A configurable simulation environment

Scenario-based testing

Control systems for observing different fluid behaviors

---

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


### Kernels for numerical approximation
 - Muller's Poly6 Kernel
 - Spiky Kernel
 - Viscosity Kernel

