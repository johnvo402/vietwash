import { test, expect } from '@playwright/test';

test('test-create-inv-docs', async ({ page }) => {
  await page.goto('http://localhost:3000/manage/inventory/import');
  await expect(page.getByLabel('breadcrumb').getByRole('link', { name: 'Nhập kho' })).toBeVisible();
  await page.getByRole('button', { name: 'Tạo mới' }).click();
  await page.getByRole('combobox').filter({ hasText: 'Chọn vật tư' }).click();
  await page.getByText('Găng tay cao su').click();
  await page.getByRole('combobox').filter({ hasText: 'Chọn đơn vị' }).click();
  await page.getByLabel('Thùng').getByText('Thùng').click();
  await page.getByText('Chọn nhà cung cấp').click();
  await page.getByText('Siêu Thị GO! VIETNAM').click();
  await page.getByRole('row', { name: '40.634' }).getByRole('button').nth(1).click();
  await page.getByRole('row', { name: '40.634' }).getByRole('button').nth(1).dblclick();
  await page.getByRole('row', { name: '40.634' }).getByRole('button').nth(1).click();
  await page.getByRole('row', { name: '40.634' }).getByRole('button').nth(1).click();
  await page.getByRole('textbox', { name: 'Thời gian giao dịch' }).click();
  await page.getByRole('textbox', { name: 'Thời gian giao dịch' }).fill('2025-07-21T22:44');
  await page.getByRole('button', { name: 'Gửi' }).click();
  await expect(page.getByText('Cập nhật phiếu kho thành công')).toBeVisible();
});