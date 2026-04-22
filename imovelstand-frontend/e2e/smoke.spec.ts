import { expect, test } from '@playwright/test';

/**
 * Smoke tests: apenas validam que as páginas públicas carregam sem erro.
 * Testes com fluxo autenticado ficam em separado (precisam do backend rodando).
 */
test.describe('Smoke — páginas públicas', () => {
  test('Landing carrega com CTA', async ({ page }) => {
    await page.goto('/landing');
    await expect(page).toHaveTitle(/ImovelStand/);
    await expect(page.getByRole('heading', { name: /Venda mais imóveis/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /Testar grátis/i })).toBeVisible();
  });

  test('Onboarding mostra stepper', async ({ page }) => {
    await page.goto('/onboarding');
    await expect(page.getByRole('heading', { name: /Comece agora/i })).toBeVisible();
    await expect(page.getByLabel('Nome da empresa')).toBeVisible();
  });

  test('Login mostra campo de email', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByRole('heading', { name: /Entrar no ImovelStand/i })).toBeVisible();
    await expect(page.getByLabel('Email')).toBeVisible();
  });

  test('Rota protegida redireciona pra login', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL(/\/login$/);
  });
});
