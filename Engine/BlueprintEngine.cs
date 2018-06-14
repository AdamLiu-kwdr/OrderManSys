using System;
using System.Collections.Generic;
using System.Linq;
using OrderManSys.Model;
using OrderManSys.Repository;

namespace OrderManSys.Engine
{
    //This is the Analyze engine's code.
    //Engine will accept a Product type and return all the instruction at once. 
    //(If the production's instruction are registered in the database.)
    public class BlueprintEngine
    {
        private readonly InstructionRepo instructionRepo;
        public BlueprintEngine(ConnectionStringOption options)
        {
            instructionRepo = new InstructionRepo(options.Factory);
        }

        public IEnumerable<Instruction> GetRegistred(Product product)
        {
            var QuerryParameters = new Dictionary<string,object>();
            QuerryParameters.Add("Product",product.Id); 
            var InstructionSets = instructionRepo.Get(QuerryParameters);

            //If the product haven't register it's instruction yet. 
            if (InstructionSets.Count() == 0)
            {
                throw new ArgumentException($"Product: {product.ProductName} is not registered in BluePrint Engine.");
            }

            return(InstructionSets);
        }
    }
}