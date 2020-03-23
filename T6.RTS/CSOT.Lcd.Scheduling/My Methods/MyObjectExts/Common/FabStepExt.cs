using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using CSOT.Lcd.Scheduling.DataModel;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Persists;
namespace CSOT.Lcd.Scheduling
{
	[FeatureBind()]
	public static partial class FabStepExt
	{
		/// <param name="isMain">true: MainL-ine, false:U-Line</param>
		public static string GetTatKey(string productID, bool isMain)
		{
			return LcdHelper.CreateKey(productID, isMain.ToString());
		}

		public static void AddStepTat(this FabStep step, StepTat tat)
		{
			string key = GetTatKey(tat.ProductID, tat.IsMain);

			if (step.StepTats.ContainsKey(key) == false)
				step.StepTats.Add(key, tat);
		}

		public static StepTat GetTat(this FabStep step, string productID, bool isMain)
		{
			string key = GetTatKey(productID, isMain);

			StepTat tat;
			step.StepTats.TryGetValue(key, out tat);

			return tat;
		}

		public static StepTat GetDefaultTAT(this FabStep step)
		{
			if (step.DefaultTAT == null)
			{
				StepTat tat = new StepTat();
				tat.WaitTat = (float)SiteConfigHelper.GetDefaultWaitTAT().TotalMinutes;
				tat.RunTat = (float)SiteConfigHelper.GetDefaultRunTAT().TotalMinutes;
				tat.TAT = tat.WaitTat + tat.RunTat;

				step.DefaultTAT = tat;
			}

			return step.DefaultTAT;
		}

		public static void AddYield(this FabStep step, string productID, float yield)
		{
			if (step.Yields.ContainsKey(productID) == false)
				step.Yields.Add(productID, yield);
		}

		public static float GetYield(this FabStep step, string productID)
		{
			float yield;
			if (step.Yields.TryGetValue(productID, out yield))
				return yield;

			return 1f;
		}

		public static List<FabStep> GetPrevSteps(this FabStep currnetStep, string productID)
		{
			List<FabStep> result = new List<FabStep>();

			if (currnetStep.IsArrayShop && currnetStep.IsFirst)
				return result;

			foreach (FabStep item in currnetStep.GetInnerPrevSteps())
				result.Add(item);

			FabProduct prod = BopHelper.FindProduct(currnetStep.ShopID, productID);

			FabInterBom bom;
			prod.TryGetPrevInterRoute(currnetStep, out bom);

			if (bom != null)
				result.Add(bom.ChangeStep);

			return result;
		}

		public static FabStep GetPrevMainStep(this FabStep currentStep, FabProduct product, bool checkInterBom)
		{
			FabStep orgStep = currentStep;
			FabInterBom interBoms = null;

			if (checkInterBom && product.TryGetPrevInterRoute(orgStep, out interBoms))
				return interBoms.ChangeStep;

			FabStep prev = currentStep.PrevStep as FabStep;
			do
			{
				if (prev == null)
					return null;

				if (IsSkipStep(product, prev))
				{
					if (prev.PrevStep != null)
						prev = prev.PrevStep as FabStep;
					else
						prev = null;

					continue;
				}

				break;

			} while (true);

			return prev;
		}

		public static FabStep GetNextStep(this FabStep step, FabProduct product, ref FabProduct prod)
		{
			/*0. TFT_CF MODE 로 Simualtion run 중일 때 현재 Step이 StrCellInBankStepID 이면 종료
				0-1. 현재 Step 이 IntershopBom FromStep 인 경우 변경 후 종료 
			  1. 다음 Step 을 받아와서 다음 Step 이 Null 인 경우
				1-1. 제품의 Main Process 와 현재 Lot 의 Process 가 다른지 확인
					->. 다른 경우 SubProcess 이며 이경우에는 
					-> 이전 Step 을 기준으로 Return Step을 찾아 반환 후 종료
					-> Retern Step 을 못 찾는 경우에는 Next Step 으로 null 반환
				1-2. Main Process 상에 있는 경우 NextStep 으로 Null 을 반환하여 Lot 을 Sink 시킴(예외, 정상적인 경우 이런 데이터 나오면 안됨)

			  2. 다음 Step 이 Null 이 아닌 경우
				2-1. Assy 대상 공정(=Cell 투입 공정)인 경우 다음 Step 설정 후 종료                
				2-2. 일반 Step인 경우에는 Skip 여부를 판별하여 Skip 인 경우 Step1 부터 다시 판별
			*/

			if (SimHelper.IsTftRunning)
			{
				if (step != null && step.ShopID == Constants.CellShop
					&& step.StepID == Constants.CellInStepID)
				{
					step = null;

					return step;
				}
			}

			FabStep orgStep = step;
			FabInterBom interBoms;
			if (product.TryGetNextInterRoute(orgStep, out interBoms))
			{
				step = interBoms.ChangeStep;
				prod = interBoms.ChangeProduct;

				return step;
			}

			FabStep next = step.NextStep as FabStep;
			do
			{
				if (next == null)
				{
					step = next;

					if (step == null)
						return step;
				}

				if (IsSkipStep(product, next))
				{
					if (next.NextStep != null)
						next = next.NextStep as FabStep;
					else
						next = null;

					continue;
				}

				break;

			} while (true);

			return next;
		}

		internal static bool IsSkipStep(FabProduct prod, FabStep step)
		{
			//if (step.StepType == "MAIN")
			//	return false;

			return false;
		}

		public static List<FabStep> GetNextStepList(this FabStep baseStep, FabProduct baseProduct, int stepCnt = 999)
		{
			List<FabStep> list = new List<FabStep>();

			FabStep currStep = baseStep;
			FabProduct currProd = baseProduct;

			int cnt = 0;
			while (cnt < stepCnt)
			{
				FabStep nextStep = currStep.GetNextStep(currProd, ref currProd);
				if (nextStep == null)
					break;

				list.Add(nextStep);

				currStep = nextStep;
				cnt++;
			}

			return list;
		}

		/// <summary>
		/// 가까운 PhotoStep을 찾습니다. 
		/// </summary>
		/// <returns>찾지못할 경우 Null이 반환됩니다.</returns>
		public static FabStep GetNextPhotoNearByMe(this FabStep baseStep, FabProduct baseProduct, int stepCnt, out int idx)
		{
			FabStep currStep = baseStep;
			FabProduct currProd = baseProduct;

			int cnt = 0;
			while (cnt < stepCnt)
			{
				FabStep nextStep = currStep.GetNextStep(currProd, ref currProd);
				if (nextStep == null)
					break;

				if (nextStep.StdStep.IsPhoto)
				{
					idx = cnt;
					return nextStep;
				}

				currStep = nextStep;

				//count : only MandatoryStep
				if (nextStep.IsMandatoryStep)
					cnt++;
			}

			idx = 0;
			return null;
		}

	}
}
